using AcademicManagement.Domain;
using AcademicManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace AcademicManagement.Controllers;

[ApiController]
[Route("api")]
public class AvailabilityController(
    AvailabilityService availability, AvailabilityParser parser,
    AcademicService academic, WorkloadService workload) : ControllerBase
{
    /// <summary>
    /// Verifică o repartizare FĂRĂ s-o salveze: reguli (calificare, limbă, curriculum, normă) +
    /// norma anuală rezultată + dacă orele încap în disponibilitatea declarată.
    /// Erorile blochează salvarea; avertismentele (norme neconfigurate, disponibilitate insuficientă) nu o blochează.
    /// </summary>
    [HttpPost("teachingassignments/check")]
    public async Task<IActionResult> CheckAssignment([FromBody] TeachingAssignment assignment)
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        WorkloadResult? load = null;
        List<FeasibilityRow> feasibility = [];

        try { WorkloadService.AcademicYearRange(assignment.AcademicYear ?? ""); }
        catch (ArgumentException ex)
        {
            return Ok(new AssignmentCheck(false, [ex.Message.Split(" (Parameter")[0]], null, [], []));
        }

        try { await academic.ValidateAssignmentAsync(assignment); }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        { errors.Add(ex.Message); }
        catch (KeyNotFoundException ex) { errors.Add(ex.Message); }

        try
        {
            if (assignment.Hours > 0 && assignment.Semester is 1 or 2)
            {
                load = await workload.CalculateAsync(assignment.ProfessorProfileId, assignment.AcademicYear,
                    (assignment.ActivityType, assignment.Hours, assignment.Semester));
                warnings.AddRange(load.Warnings);
                feasibility = await availability.FeasibilityAsync(assignment.ProfessorProfileId, assignment.AcademicYear,
                    (assignment.Semester, assignment.Hours));
                warnings.AddRange(feasibility.Where(f => !f.Ok).Select(f => $"Semestrul {f.Semester}: {f.Message}"));
            }
        }
        catch (Exception ex) when (ex is KeyNotFoundException or ArgumentException)
        { if (!errors.Contains(ex.Message)) errors.Add(ex.Message); }

        return Ok(new AssignmentCheck(errors.Count == 0, errors, load, feasibility, warnings));
    }

    /// <summary>Profesorii departamentului ale căror ore nu încap în disponibilitatea declarată.</summary>
    [HttpGet("departments/{departmentId:guid}/feasibility")]
    public Task<IActionResult> DepartmentFeasibility(Guid departmentId, [FromQuery] string academicYear) =>
        Run(async () => await availability.DepartmentFeasibilityAsync(departmentId, academicYear));

    [HttpGet("time-slots")]
    public async Task<IActionResult> TimeSlots() =>
        Ok((await availability.SlotsAsync()).Select(s => new SlotDto(s.SlotNumber, s.StartTime.ToString(@"hh\:mm"), s.EndTime.ToString(@"hh\:mm"), s.IsLunchBreak)));

    [HttpGet("professors/{professorId:guid}/availability")]
    public Task<IActionResult> Get(Guid professorId, [FromQuery] string academicYear) =>
        Run(async () => await availability.GetAsync(professorId, academicYear));

    [HttpPut("professors/{professorId:guid}/availability")]
    public Task<IActionResult> Save(Guid professorId, [FromBody] AvailabilityDto dto) =>
        Run(async () => { await availability.SaveAsync(professorId, dto); return new { Saved = true }; });

    [HttpGet("professors/{professorId:guid}/availability/feasibility")]
    public Task<IActionResult> Feasibility(Guid professorId, [FromQuery] string academicYear) =>
        Run(async () => await availability.FeasibilityAsync(professorId, academicYear));

    /// <summary>Propune o grilă pornind de la un text liber. NU salvează nimic.</summary>
    [HttpPost("professors/{professorId:guid}/availability/parse")]
    public Task<IActionResult> Parse(Guid professorId, [FromBody] ParseRequest request) =>
        Run(async () => await parser.ParseAsync(request.Text, await availability.SlotsAsync()));

    private async Task<IActionResult> Run<T>(Func<Task<T>> action)
    {
        try { return Ok(await action()); }
        catch (KeyNotFoundException ex) { return NotFound(new { Error = ex.Message }); }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        { return BadRequest(new { Error = ex.Message }); }
    }
}

public record ParseRequest(string Text);
public record AssignmentCheck(bool Ok, List<string> Errors, WorkloadResult? Workload, List<FeasibilityRow> Feasibility, List<string> Warnings);
