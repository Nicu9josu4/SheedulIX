using AcademicManagement.Data;
using AcademicManagement.Domain;
using AcademicManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicManagement.Controllers;

[ApiController]
[Route("api")]
public class TimetableController(AppDbContext context, TimetableService timetable, TimetableExportService export) : ControllerBase
{
    // ===== Export =====
    /// <summary>Excel cu orarul pe grupe, profesori și săli, plus perechile neplasate.</summary>
    [HttpGet("timetable/export/excel")]
    public async Task<IActionResult> ExportExcel([FromQuery] string academicYear, [FromQuery] int semester)
    {
        try
        {
            var bytes = await export.ExcelAsync(academicYear, semester);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"orar-{academicYear}-sem{semester}.xlsx");
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        { return BadRequest(new { Error = ex.Message.Split(" (Parameter")[0] }); }
    }

    /// <summary>PDF gata de tipar: câte o pagină pe grupă (implicit), profesor sau sală; <c>id</c> restrânge la una singură.</summary>
    [HttpGet("timetable/export/pdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] string academicYear, [FromQuery] int semester,
        [FromQuery] string view = "group", [FromQuery] Guid? id = null)
    {
        if (!Enum.TryParse<ExportView>(view, true, out var v))
            return BadRequest(new { Error = "Vizualizarea trebuie să fie group, professor, room sau all." });
        try
        {
            var bytes = await export.PdfAsync(academicYear, semester, v, id);
            return File(bytes, "application/pdf", $"orar-{academicYear}-sem{semester}-{view}.pdf");
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        { return BadRequest(new { Error = ex.Message.Split(" (Parameter")[0] }); }
    }

    // ===== Săli =====
    [HttpGet("rooms")]
    public async Task<IActionResult> GetRooms() =>
        Ok(await context.Rooms.OrderBy(r => r.Building).ThenBy(r => r.Code).ToListAsync());

    [HttpPost("rooms")]
    public async Task<IActionResult> CreateRoom([FromBody] Room room)
    {
        room.Code = (room.Code ?? "").Trim(); room.Building = (room.Building ?? "").Trim();
        if (room.Code == "") return BadRequest(new { Error = "Codul sălii este obligatoriu." });
        if (room.Building == "") return BadRequest(new { Error = "Corpul (clădirea) este obligatoriu." });
        if (room.Capacity <= 0) return BadRequest(new { Error = "Capacitatea trebuie să fie pozitivă." });
        if (await context.Rooms.AnyAsync(r => r.Building == room.Building && r.Code == room.Code))
            return Conflict(new { Error = "Există deja o sală cu acest cod în acest corp." });
        room.Id = Guid.NewGuid();
        context.Rooms.Add(room);
        await context.SaveChangesAsync();
        return Ok(room);
    }

    /// <summary>Marchează o sală activă / indisponibilă (reparație).</summary>
    [HttpPost("rooms/{roomId:guid}/active")]
    public async Task<IActionResult> SetRoomActive(Guid roomId, [FromBody] bool active)
    {
        var room = await context.Rooms.FindAsync(roomId);
        if (room is null) return NotFound();
        room.Active = active;
        await context.SaveChangesAsync();
        return Ok(room);
    }

    [HttpDelete("rooms/{roomId:guid}")]
    public async Task<IActionResult> DeleteRoom(Guid roomId)
    {
        var room = await context.Rooms.FindAsync(roomId);
        if (room is null) return NotFound();
        if (await context.TimetableEntries.AnyAsync(e => e.RoomId == roomId))
            return Conflict(new { Error = "Sala apare în orarul generat. Marc-o ca indisponibilă în loc să o ștergi." });
        context.Rooms.Remove(room);
        await context.SaveChangesAsync();
        return NoContent();
    }

    // ===== Orar =====
    /// <summary>Generează orarul pentru un an și un semestru (înlocuiește orarul anterior al aceluiași semestru).</summary>
    [HttpPost("timetable/generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateTimetableRequest request)
    {
        try { return Ok(await timetable.GenerateAsync(request.AcademicYear, request.Semester, request.TimeLimitSeconds ?? 20)); }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        { return BadRequest(new { Error = ex.Message.Split(" (Parameter")[0] }); }
    }

    [HttpGet("timetable/summary")]
    public async Task<IActionResult> Summary([FromQuery] string academicYear, [FromQuery] int semester)
    {
        var s = await timetable.LatestSummaryAsync(academicYear, semester);
        return s is null ? NotFound(new { Error = "Nu există încă un orar generat pentru acest an și semestru." }) : Ok(s);
    }

    [HttpGet("timetable")]
    public async Task<IActionResult> Entries([FromQuery] string academicYear, [FromQuery] int semester,
        [FromQuery] Guid? groupId, [FromQuery] Guid? professorId, [FromQuery] Guid? roomId) =>
        Ok(await timetable.EntriesAsync(academicYear, semester, groupId, professorId, roomId));
}

public record GenerateTimetableRequest(string AcademicYear, int Semester, int? TimeLimitSeconds);
