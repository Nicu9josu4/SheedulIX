using AcademicManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace AcademicManagement.Controllers;

[ApiController]
[Route("api")]
public class AvailabilityController(AvailabilityService availability, AvailabilityParser parser) : ControllerBase
{
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
