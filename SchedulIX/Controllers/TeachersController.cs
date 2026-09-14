using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchedulIX.Data;
using SchedulIX.Models.DTOs;

namespace SchedulIX.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeachersController : ControllerBase
    {
        private readonly ScheduleDbContext _db;

        public TeachersController(ScheduleDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<TeacherDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var teachers = await _db.Teachers.AsNoTracking().ToListAsync(ct);
            var result = teachers.Select(t => new TeacherDto(t.Id, t.FirstName, t.LastName, t.Email)).ToList();
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(TeacherDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var t = await _db.Teachers.FindAsync(new object[] { id }, ct);
            if (t is null) return NotFound(new { Message = $"Profesorul cu ID {id} nu a fost găsit." });
            return Ok(new TeacherDto(t.Id, t.FirstName, t.LastName, t.Email));
        }

        [HttpPost]
        [ProducesResponseType(typeof(TeacherDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateTeacherRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.FirstName) || string.IsNullOrWhiteSpace(req.LastName) || string.IsNullOrWhiteSpace(req.Email))
                return BadRequest(new { Message = "Invalid teacher data." });

            var exists = await _db.Teachers.AnyAsync(x => x.Email == req.Email, ct);
            if (exists) return BadRequest(new { Message = "Email already in use." });

            var teacher = new Models.Entities.Teacher
            {
                FirstName = req.FirstName,
                LastName = req.LastName,
                Email = req.Email
            };

            _db.Teachers.Add(teacher);
            await _db.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(GetById), new { id = teacher.Id }, new TeacherDto(teacher.Id, teacher.FirstName, teacher.LastName, teacher.Email));
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(TeacherDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateTeacherRequest req, CancellationToken ct)
        {
            var t = await _db.Teachers.FindAsync(new object[] { id }, ct);
            if (t is null) return NotFound(new { Message = $"Teacher {id} not found." });

            var exists = await _db.Teachers.AnyAsync(x => x.Email == req.Email && x.Id != id, ct);
            if (exists) return BadRequest(new { Message = "Email already in use." });

            t.FirstName = req.FirstName;
            t.LastName = req.LastName;
            t.Email = req.Email;

            await _db.SaveChangesAsync(ct);

            return Ok(new TeacherDto(t.Id, t.FirstName, t.LastName, t.Email));
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var t = await _db.Teachers.FindAsync(new object[] { id }, ct);
            if (t is null) return NotFound(new { Message = $"Teacher {id} not found." });

            _db.Teachers.Remove(t);
            await _db.SaveChangesAsync(ct);

            return NoContent();
        }
    }
}
