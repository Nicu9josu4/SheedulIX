using AcademicManagement.Data;
using AcademicManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace AcademicManagement.Services;

public class AcademicService
{
    private readonly AppDbContext _context;
    private readonly WorkloadService _workload;

    public AcademicService(AppDbContext context, WorkloadService workload)
    {
        _context = context;
        _workload = workload;
    }

    /// <summary>
    /// Generates maximum 2 balanced subgroups if student count > 26.
    /// </summary>
    public async Task SynchronizeSubgroupsAsync(Guid groupId)
    {
        var group = await _context.Groups
            .Include(g => g.Subgroups)
            .FirstOrDefaultAsync(g => g.Id == groupId)
            ?? throw new KeyNotFoundException("Grupa nu a fost găsită.");

        _context.Subgroups.RemoveRange(group.Subgroups);

        if (group.StudentCount > 26)
        {
            int baseCount = group.StudentCount / 2;
            int remainder = group.StudentCount % 2;

            var sg1 = new Subgroup { GroupId = groupId, Name = $"{group.Code}-SG1", StudentCount = baseCount + remainder };
            var sg2 = new Subgroup { GroupId = groupId, Name = $"{group.Code}-SG2", StudentCount = baseCount };

            _context.Subgroups.AddRange(sg1, sg2);
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Regula de atribuire a curriculumului: cohorta care incepe studiile in anul X primeste curriculumul
    /// activ al programului cu versiunea X-Y; daca nu exista, primeste ultimul curriculum creat al programului.
    /// </summary>
    public async Task<(Curriculum? Curriculum, bool UsedFallback)> ResolveCurriculumAsync(Guid studyProgramId, int entryYear)
    {
        var active = await _context.Curriculums
            .Where(c => c.StudyProgramId == studyProgramId && c.IsActive)
            .ToListAsync();

        var exact = active
            .Where(c => StartYear(c.Version) == entryYear)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefault();
        if (exact is not null) return (exact, false);

        var latest = active.OrderByDescending(c => c.CreatedAt).FirstOrDefault();
        return (latest, latest is not null);
    }

    /// <summary>"2026-2027" → 2026; null daca versiunea nu incepe cu un an.</summary>
    private static int? StartYear(string version)
    {
        var m = System.Text.RegularExpressions.Regex.Match(version ?? "", @"(20\d{2})");
        return m.Success ? int.Parse(m.Groups[1].Value) : null;
    }

    /// <summary>Validates business rules and saves the teaching assignment.</summary>
    public async Task AssignProfessorAsync(TeachingAssignment assignment)
    {
        await ValidateAssignmentAsync(assignment);
        _context.TeachingAssignments.Add(assignment);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Validates business rules prior to creating a teaching assignment (nothing is saved).
    /// Throws InvalidOperationException with the reason, or KeyNotFoundException for a missing group.
    /// </summary>
    public async Task ValidateAssignmentAsync(TeachingAssignment assignment)
    {
        if (assignment.Hours <= 0)
            throw new InvalidOperationException("Numărul de ore trebuie să fie pozitiv.");
        if (string.IsNullOrWhiteSpace(assignment.AcademicYear))
            throw new InvalidOperationException("Anul universitar este obligatoriu (ex: 2026-2027).");
        if (assignment.Semester is < 1 or > 2)
            throw new InvalidOperationException("Semestrul trebuie să fie 1 sau 2.");

        // 1. Rule: Lectures cannot be assigned to subgroups
        if (assignment.ActivityType == ActivityType.Lecture && assignment.SubgroupId.HasValue)
        {
            throw new InvalidOperationException("Cursurile nu pot fi împărțite pe subgrupe.");
        }

        // 2. Validate Professor Skills (Subject Capability)
        bool canTeachSubject = await _context.Set<ProfessorSubject>()
            .AnyAsync(ps => ps.ProfessorProfileId == assignment.ProfessorProfileId && ps.SubjectId == assignment.SubjectId);

        if (!canTeachSubject)
        {
            throw new InvalidOperationException("Profesorul nu este calificat pentru această disciplină.");
        }

        // 3. Validate Language Compatibility
        bool canTeachLanguage = await _context.Set<ProfessorLanguage>()
            .AnyAsync(pl => pl.ProfessorProfileId == assignment.ProfessorProfileId && pl.LanguageId == assignment.LanguageId);

        if (!canTeachLanguage)
        {
            throw new InvalidOperationException("Profesorul nu este autorizat să predea în această limbă.");
        }

        // 4. Validate Curriculum Inclusion
        var group = await _context.Groups.FindAsync(assignment.GroupId)
            ?? throw new KeyNotFoundException("Grupa nu a fost găsită.");

        bool subjectInCurriculum = await _context.CurriculumSubjects
            .AnyAsync(cs => cs.CurriculumId == group.CurriculumId
                         && cs.SubjectId == assignment.SubjectId
                         && cs.LanguageId == assignment.LanguageId);

        if (!subjectInCurriculum)
        {
            throw new InvalidOperationException("Disciplina cu această limbă nu există în curriculumul grupei.");
        }

        // 5. Rule: sarcina totala a profesorului trebuie sa ramana in limitele normei / contractului
        await _workload.EnsureWithinLimitsAsync(assignment);
    }
}