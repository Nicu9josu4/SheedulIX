using AcademicManagement.Data;
using AcademicManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace AcademicManagement.Services;

/// <summary>Fluxul de aprobare a statului de functii al unui departament.</summary>
public class StaffingPlanService(AppDbContext context, WorkloadService workload)
{
    // Draft → Submitted → DepartmentApproved → FacultyApproved → Approved ; orice pas de aprobare poate fi Rejected ; Rejected → Draft
    private static readonly Dictionary<StaffingPlanStatus, StaffingPlanStatus[]> Transitions = new()
    {
        [StaffingPlanStatus.Draft] = [StaffingPlanStatus.Submitted],
        [StaffingPlanStatus.Submitted] = [StaffingPlanStatus.DepartmentApproved, StaffingPlanStatus.Rejected],
        [StaffingPlanStatus.DepartmentApproved] = [StaffingPlanStatus.FacultyApproved, StaffingPlanStatus.Rejected],
        [StaffingPlanStatus.FacultyApproved] = [StaffingPlanStatus.Approved, StaffingPlanStatus.Rejected],
        [StaffingPlanStatus.Approved] = [],
        [StaffingPlanStatus.Rejected] = [StaffingPlanStatus.Draft],
    };

    public async Task<StaffingPlan> CreateAsync(Guid departmentId, string academicYear)
    {
        WorkloadService.AcademicYearRange(academicYear); // valideaza formatul
        if (!await context.Departments.AnyAsync(d => d.Id == departmentId))
            throw new KeyNotFoundException("Departamentul nu a fost găsit.");
        if (await context.StaffingPlans.AnyAsync(p => p.DepartmentId == departmentId && p.AcademicYear == academicYear))
            throw new InvalidOperationException("Există deja un stat de funcții pentru acest departament și an.");

        var plan = new StaffingPlan { DepartmentId = departmentId, AcademicYear = academicYear };
        context.StaffingPlans.Add(plan);
        await context.SaveChangesAsync();
        return plan;
    }

    public async Task<StaffingPlan> TransitionAsync(Guid planId, StaffingPlanStatus to, string actor, string? comment)
    {
        var plan = await context.StaffingPlans.FirstOrDefaultAsync(p => p.Id == planId)
            ?? throw new KeyNotFoundException("Statul de funcții nu a fost găsit.");

        if (!Transitions[plan.Status].Contains(to))
            throw new InvalidOperationException($"Tranziția {plan.Status} → {to} nu este permisă.");
        if (string.IsNullOrWhiteSpace(actor))
            throw new InvalidOperationException("Trebuie specificat cine efectuează acțiunea.");
        if (to == StaffingPlanStatus.Rejected && string.IsNullOrWhiteSpace(comment))
            throw new InvalidOperationException("Respingerea necesită un comentariu.");

        // La depunere, nimeni din departament nu are voie sa depaseasca limitele legale.
        if (to == StaffingPlanStatus.Submitted)
        {
            var problems = await HardViolationsAsync(plan);
            if (problems.Count > 0)
                throw new InvalidOperationException("Statul nu poate fi depus: " + string.Join(" | ", problems));
        }

        context.ApprovalSteps.Add(new ApprovalStep
        {
            StaffingPlanId = plan.Id, FromStatus = plan.Status, ToStatus = to,
            ActorName = actor, Comment = comment
        });
        plan.Status = to;
        plan.UpdatedAt = DateTime.UtcNow;

        // La aprobarea finala, repartizarile departamentului pe acel an devin Approved.
        if (to == StaffingPlanStatus.Approved)
        {
            var deptPersonIds = context.DepartmentEmployments
                .Where(e => e.DepartmentId == plan.DepartmentId && e.Active)
                .Select(e => e.PersonId);
            var toApprove = await context.TeachingAssignments
                .Where(a => a.AcademicYear == plan.AcademicYear
                         && a.Status == AssignmentStatus.Assigned
                         && deptPersonIds.Contains(a.ProfessorProfile.PersonId))
                .ToListAsync();
            foreach (var a in toApprove) a.Status = AssignmentStatus.Approved;
        }

        await context.SaveChangesAsync();
        return plan;
    }

    /// <summary>Calculul sarcinii pentru toti profesorii departamentului (ambele semestre).</summary>
    public async Task<List<WorkloadResult>> DepartmentWorkloadAsync(Guid departmentId, string academicYear)
    {
        var profileIds = await context.ProfessorProfiles
            .Where(p => p.Person.Employments.Any(e => e.DepartmentId == departmentId && e.Active))
            .Select(p => p.Id)
            .ToListAsync();

        var results = new List<WorkloadResult>();
        foreach (var id in profileIds)
            foreach (var semester in new[] { 1, 2 })
                results.Add(await workload.CalculateAsync(id, academicYear, semester));
        return results;
    }

    private async Task<List<string>> HardViolationsAsync(StaffingPlan plan)
    {
        var all = await DepartmentWorkloadAsync(plan.DepartmentId, plan.AcademicYear);
        return all.Where(r => r.Violations.Count > 0)
                  .Select(r => $"{r.FullName} (sem. {r.Semester}): {string.Join(" ", r.Violations)}")
                  .ToList();
    }
}
