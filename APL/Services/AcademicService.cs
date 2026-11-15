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
            ?? throw new KeyNotFoundException("Group not found");

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

    /// <summary>
    /// Validates business rules prior to creating a teaching assignment.
    /// </summary>
    public async Task AssignProfessorAsync(TeachingAssignment assignment)
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
            throw new InvalidOperationException("Lectures cannot be split into subgroups.");
        }

        // 2. Validate Professor Skills (Subject Capability)
        bool canTeachSubject = await _context.Set<ProfessorSubject>()
            .AnyAsync(ps => ps.ProfessorProfileId == assignment.ProfessorProfileId && ps.SubjectId == assignment.SubjectId);

        if (!canTeachSubject)
        {
            throw new InvalidOperationException("Professor is not qualified to teach this subject.");
        }

        // 3. Validate Language Compatibility
        bool canTeachLanguage = await _context.Set<ProfessorLanguage>()
            .AnyAsync(pl => pl.ProfessorProfileId == assignment.ProfessorProfileId && pl.LanguageId == assignment.LanguageId);

        if (!canTeachLanguage)
        {
            throw new InvalidOperationException("Professor is not authorized to teach in this language.");
        }

        // 4. Validate Curriculum Inclusion
        var group = await _context.Groups.FindAsync(assignment.GroupId)
            ?? throw new KeyNotFoundException("Group not found");

        bool subjectInCurriculum = await _context.CurriculumSubjects
            .AnyAsync(cs => cs.CurriculumId == group.CurriculumId
                         && cs.SubjectId == assignment.SubjectId
                         && cs.LanguageId == assignment.LanguageId);

        if (!subjectInCurriculum)
        {
            throw new InvalidOperationException("Subject/Language combination is not present in group's curriculum.");
        }

        // 5. Rule: sarcina totala a profesorului trebuie sa ramana in limitele normei / contractului
        await _workload.EnsureWithinLimitsAsync(assignment);

        _context.TeachingAssignments.Add(assignment);
        await _context.SaveChangesAsync();
    }
}