using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchedulIX.Data;
using SchedulIX.Models.DTOs;

namespace SchedulIX.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupsController : ControllerBase
    {
        private readonly ScheduleDbContext _db;

        public GroupsController(ScheduleDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<AcademicGroupDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var groups = await _db.AcademicGroups
                .AsNoTracking()
                .Select(g => new AcademicGroupDto(g.Id, g.Name, g.EducationFormId, g.SeriesId))
                .ToListAsync(ct);

            return Ok(groups);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(AcademicGroupDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var g = await _db.AcademicGroups.FindAsync(new object[] { id }, ct);
            if (g is null) return NotFound(new { Message = $"Grupa cu ID {id} nu a fost găsită." });
            return Ok(new AcademicGroupDto(g.Id, g.Name, g.EducationFormId, g.SeriesId));
        }

        [HttpPost]
        [ProducesResponseType(typeof(AcademicGroupDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateAcademicGroupRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return BadRequest(new { Message = "Invalid group name." });

            // verify education form exists
            var ef = await _db.EducationForms.FindAsync(new object[] { req.EducationFormId }, ct);
            if (ef is null) return BadRequest(new { Message = $"EducationForm {req.EducationFormId} not found." });

            var group = new Models.Entities.AcademicGroup
            {
                Name = req.Name,
                EducationFormId = req.EducationFormId,
                SeriesId = req.SeriesId
            };

            if (req.Subgroups?.Any() == true)
            {
                group.Subgroups = req.Subgroups.Select(s => new Models.Entities.Subgroup { Name = s.Name }).ToList();
            }

            _db.AcademicGroups.Add(group);
            await _db.SaveChangesAsync(ct);

            var dto = new AcademicGroupDto(group.Id, group.Name, group.EducationFormId, group.SeriesId);
            return CreatedAtAction(nameof(GetById), new { id = group.Id }, dto);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(AcademicGroupDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateAcademicGroupRequest req, CancellationToken ct)
        {
            var g = await _db.AcademicGroups.FindAsync(new object[] { id }, ct);
            if (g is null) return NotFound(new { Message = $"Group {id} not found." });

            if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest(new { Message = "Invalid group name." });

            g.Name = req.Name;
            g.EducationFormId = req.EducationFormId;
            g.SeriesId = req.SeriesId;

            await _db.SaveChangesAsync(ct);

            return Ok(new AcademicGroupDto(g.Id, g.Name, g.EducationFormId, g.SeriesId));
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var g = await _db.AcademicGroups.FindAsync(new object[] { id }, ct);
            if (g is null) return NotFound(new { Message = $"Group {id} not found." });

            _db.AcademicGroups.Remove(g);
            await _db.SaveChangesAsync(ct);

            return NoContent();
        }
    }
}
