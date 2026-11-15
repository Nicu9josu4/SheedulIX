using AcademicManagement.Data;
using AcademicManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace AcademicManagement.Services;

public record WorkloadResult(
    Guid ProfessorProfileId,
    string FullName,
    string AcademicYear,
    string? Position,
    EmploymentType? EmploymentType,
    int AssignedPhysicalHours,
    decimal ConventionalHoursSemester1,
    decimal ConventionalHoursSemester2,
    decimal ConventionalHoursYear,   // ore conventionale pe an universitar (ambele semestre)
    decimal? BaseNormHours,
    decimal? MinHours,
    decimal? MaxHours,
    decimal? Delta,                  // fata de norma de baza (+ excedent, - deficit)
    string Status,                   // Ok | Deficit | Exceeded | NotConfigured
    IReadOnlyList<string> Violations,
    IReadOnlyList<string> Warnings);

/// <summary>
/// Calculeaza sarcina didactica anuala (ore conventionale) a unui cadru si verifica incadrarea in limite.
/// Normele si coeficientii vin din modelul de calcul al anului (WorkloadModel), nu din cod.
/// </summary>
public class WorkloadService(AppDbContext context)
{
    /// <param name="extra">Repartizare ipotetica de adaugat (inainte de salvare).</param>
    public async Task<WorkloadResult> CalculateAsync(
        Guid professorProfileId, string academicYear,
        (ActivityType Type, int Hours, int Semester)? extra = null)
    {
        var profile = await context.ProfessorProfiles
            .Include(p => p.Person).ThenInclude(p => p.Employments)
            .FirstOrDefaultAsync(p => p.Id == professorProfileId)
            ?? throw new KeyNotFoundException("Profilul profesorului nu a fost găsit.");

        var violations = new List<string>();
        var warnings = new List<string>();

        var employments = profile.Person.Employments.Where(e => e.Active).ToList();
        var primary = employments.FirstOrDefault(e => e.EmploymentType == EmploymentType.Head)
                      ?? employments.FirstOrDefault();
        var isHead = primary?.EmploymentType == EmploymentType.Head;

        var model = await context.WorkloadModels
            .Include(m => m.Norms).Include(m => m.Coefficients)
            .FirstOrDefaultAsync(m => m.AcademicYear == academicYear);
        if (model is null)
            warnings.Add($"Nu există model de calcul pentru {academicYear}.");
        else if (model.Status == WorkloadModelStatus.Draft)
            warnings.Add($"Modelul de calcul pentru {academicYear} nu este încă activat (ciornă).");

        var norm = primary is null ? null : model?.Norms.FirstOrDefault(n => n.Position == primary.Position);
        var coefficients = model?.Coefficients.ToDictionary(c => c.ActivityType, c => c.Coefficient)
                           ?? new Dictionary<ActivityType, decimal>();

        var rows = (await context.TeachingAssignments
            .Where(a => a.ProfessorProfileId == professorProfileId
                     && a.AcademicYear == academicYear
                     && a.Status != AssignmentStatus.Cancelled
                     && a.Status != AssignmentStatus.Archived)
            .Select(a => new { a.ActivityType, a.Hours, a.Semester })
            .ToListAsync())
            .Select(a => (a.ActivityType, a.Hours, a.Semester)).ToList();
        if (extra is { } e) rows.Add(e);

        decimal Coef(ActivityType t)
        {
            if (coefficients.TryGetValue(t, out var c)) return c;
            var msg = $"Coeficientul pentru {t} în {academicYear} nu este configurat (se folosește 1).";
            if (!warnings.Contains(msg)) warnings.Add(msg);
            return 1m;
        }

        decimal Conv(int semester) => rows.Where(r => r.Semester == semester).Sum(r => r.Hours * Coef(r.ActivityType));
        decimal s1 = Conv(1), s2 = Conv(2), year = s1 + s2;
        int physical = rows.Sum(r => r.Hours);

        decimal? baseNorm = null, min = null, max = null;
        string status;

        if (primary is null)
        {
            warnings.Add("Profesorul nu are nicio angajare activă.");
            status = "NotConfigured";
        }
        else if (isHead)
        {
            if (norm is null)
            {
                warnings.Add($"Nu există normă configurată pentru funcția „{primary.Position}” în {academicYear}.");
                status = "NotConfigured";
            }
            else
            {
                baseNorm = norm.BaseNormHours; min = norm.MinHours; max = norm.MaxHours;
                if (year > max) violations.Add($"Norma depășește limita maximă ({year} > {max} ore conv./an).");
                status = year > max ? "Exceeded" : year < min ? "Deficit" : "Ok";
            }
        }
        else
        {
            // Cumul: limita este suma orelor conventionale ale contractelor active in anul universitar.
            var contractHours = await ActiveContractHoursAsync(employments.Select(x => x.Id), academicYear);
            max = contractHours;
            if (year > 0 && contractHours == 0)
            {
                violations.Add("Cadru în cumul fără contract activ: nu se pot repartiza ore.");
                status = "Exceeded";
            }
            else if (year > contractHours)
            {
                violations.Add($"Orele depășesc norma contractată ({year} > {contractHours} ore conv./an).");
                status = "Exceeded";
            }
            else status = "Ok";
        }

        return new WorkloadResult(
            professorProfileId, $"{profile.Person.LastName} {profile.Person.FirstName}", academicYear,
            primary?.Position, primary?.EmploymentType, physical,
            Math.Round(s1, 2), Math.Round(s2, 2), Math.Round(year, 2),
            baseNorm, min, max, baseNorm.HasValue ? Math.Round(year - baseNorm.Value, 2) : null,
            status, violations, warnings);
    }

    /// <summary>Arunca InvalidOperationException daca repartizarea ar depasi limita maxima.</summary>
    public async Task EnsureWithinLimitsAsync(TeachingAssignment assignment)
    {
        var result = await CalculateAsync(
            assignment.ProfessorProfileId, assignment.AcademicYear,
            (assignment.ActivityType, assignment.Hours, assignment.Semester));
        if (result.Violations.Count > 0)
            throw new InvalidOperationException(string.Join(" ", result.Violations));
    }

    private async Task<decimal> ActiveContractHoursAsync(IEnumerable<Guid> employmentIds, string academicYear)
    {
        var (from, to) = AcademicYearRange(academicYear);
        var ids = employmentIds.ToList();
        return await context.Contracts
            .Where(c => ids.Contains(c.DepartmentEmploymentId)
                     && c.Status == ContractStatus.Active
                     && c.StartDate <= to && c.EndDate >= from)
            .SumAsync(c => (decimal?)c.AnnualHours) ?? 0m;
    }

    /// <summary>"2026-2027" → 1 sept. 2026 – 31 aug. 2027 (UTC).</summary>
    public static (DateTime From, DateTime To) AcademicYearRange(string academicYear)
    {
        var parts = academicYear.Split('-');
        if (parts.Length != 2 || !int.TryParse(parts[0], out var y1) || !int.TryParse(parts[1], out var y2) || y2 != y1 + 1)
            throw new ArgumentException("Anul universitar trebuie să fie în formatul 2026-2027.", nameof(academicYear));
        return (new DateTime(y1, 9, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(y2, 8, 31, 0, 0, 0, DateTimeKind.Utc));
    }
}
