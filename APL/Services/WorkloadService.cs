using AcademicManagement.Data;
using AcademicManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace AcademicManagement.Services;

public record WorkloadResult(
    Guid ProfessorProfileId,
    string FullName,
    string AcademicYear,
    int Semester,
    string? Position,
    EmploymentType? EmploymentType,
    int AssignedPhysicalHours,
    decimal ConventionalHoursPerWeek,
    decimal? BaseNormHours,
    decimal? MinHours,
    decimal? MaxHours,
    decimal? Delta,          // ore conventionale/saptamana fata de norma de baza (+ excedent, - deficit)
    string Status,           // Ok | Deficit | Exceeded | NotConfigured
    IReadOnlyList<string> Violations,
    IReadOnlyList<string> Warnings);

/// <summary>
/// Calculeaza norma didactica (ore conventionale) a unui cadru didactic si
/// verifica incadrarea in limitele configurate. Valorile normelor si coeficientilor
/// vin din baza de date (PositionNorms / ConversionCoefficients), nu din cod.
/// </summary>
public class WorkloadService(AppDbContext context)
{
    private const int DefaultWeeks = 14;

    /// <param name="extraHours">Ore ipotetice de adaugat (ex. o repartizare noua inainte de salvare).</param>
    public async Task<WorkloadResult> CalculateAsync(
        Guid professorProfileId, string academicYear, int semester,
        (ActivityType Type, int Hours)? extraHours = null)
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

        var norm = primary is null ? null : await context.PositionNorms
            .FirstOrDefaultAsync(n => n.Position == primary.Position && n.AcademicYear == academicYear);

        var coefficients = await context.ConversionCoefficients
            .Where(c => c.AcademicYear == academicYear)
            .ToDictionaryAsync(c => c.ActivityType, c => c.Coefficient);

        var assignments = await context.TeachingAssignments
            .Where(a => a.ProfessorProfileId == professorProfileId
                     && a.AcademicYear == academicYear
                     && a.Semester == semester
                     && a.Status != AssignmentStatus.Cancelled
                     && a.Status != AssignmentStatus.Archived)
            .Select(a => new { a.ActivityType, a.Hours })
            .ToListAsync();

        var rows = assignments.Select(a => (a.ActivityType, a.Hours)).ToList();
        if (extraHours is { } extra) rows.Add(extra);

        decimal Coef(ActivityType t)
        {
            if (coefficients.TryGetValue(t, out var c)) return c;
            var msg = $"Coeficientul pentru {t} în {academicYear} nu este configurat (se folosește 1).";
            if (!warnings.Contains(msg)) warnings.Add(msg);
            return 1m;
        }

        int weeks = norm?.WeeksPerSemester > 0 ? norm.WeeksPerSemester : DefaultWeeks;
        int physical = rows.Sum(r => r.Hours);
        decimal conventional = Math.Round(rows.Sum(r => r.Hours * Coef(r.ActivityType)) / weeks, 2);

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
                if (conventional > max) violations.Add($"Norma depășește limita maximă ({conventional} > {max} ore conv./săpt.).");
                status = conventional > max ? "Exceeded" : conventional < min ? "Deficit" : "Ok";
            }
        }
        else
        {
            // Cumul: limita este data de contractele active in perioada anului universitar.
            var contractHours = await ActiveContractHoursAsync(employments.Select(e => e.Id), academicYear);
            max = contractHours;
            if (conventional > 0 && contractHours == 0)
            {
                violations.Add("Cadru în cumul fără contract activ: nu se pot repartiza ore.");
                status = "Exceeded";
            }
            else if (conventional > contractHours)
            {
                violations.Add($"Orele depășesc norma contractată ({conventional} > {contractHours} ore/săpt.).");
                status = "Exceeded";
            }
            else status = "Ok";
        }

        return new WorkloadResult(
            professorProfileId, $"{profile.Person.LastName} {profile.Person.FirstName}",
            academicYear, semester, primary?.Position, primary?.EmploymentType,
            physical, conventional, baseNorm, min, max,
            baseNorm.HasValue ? conventional - baseNorm.Value : null,
            status, violations, warnings);
    }

    /// <summary>Arunca InvalidOperationException daca repartizarea ar depasi limita maxima.</summary>
    public async Task EnsureWithinLimitsAsync(TeachingAssignment assignment)
    {
        var result = await CalculateAsync(
            assignment.ProfessorProfileId, assignment.AcademicYear, assignment.Semester,
            (assignment.ActivityType, assignment.Hours));
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
            .SumAsync(c => (decimal?)c.WeeklyHours) ?? 0m;
    }

    /// <summary>"2026-2027" → 1 sept. 2026 – 31 aug. 2027 (UTC).</summary>
    public static (DateTime From, DateTime To) AcademicYearRange(string academicYear)
    {
        var parts = academicYear.Split('-');
        if (parts.Length != 2 || !int.TryParse(parts[0], out var y1) || !int.TryParse(parts[1], out var y2))
            throw new ArgumentException("Anul universitar trebuie să fie în formatul 2026-2027.", nameof(academicYear));
        return (new DateTime(y1, 9, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(y2, 8, 31, 0, 0, 0, DateTimeKind.Utc));
    }
}
