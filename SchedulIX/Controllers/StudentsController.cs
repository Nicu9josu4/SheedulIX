using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchedulIX.Data;
using SchedulIX.Models.DTOs;
using SchedulIX.Models.Entities;

namespace SchedulIX.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly ScheduleDbContext _db;

        public StudentsController(ScheduleDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<StudentDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var students = await _db.Students.AsNoTracking().ToListAsync(ct);
            var result = students.Select(s => new StudentDto(s.Id, s.FirstName, s.LastName, s.Email, s.GroupId, s.SubgroupId)).ToList();
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var s = await _db.Students.FindAsync(new object[] { id }, ct);
            if (s is null) return NotFound(new { Message = $"Student with ID {id} not found." });
            return Ok(new StudentDto(s.Id, s.FirstName, s.LastName, s.Email, s.GroupId, s.SubgroupId));
        }

        [HttpPost]
        [ProducesResponseType(typeof(StudentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateStudentRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.FirstName) || string.IsNullOrWhiteSpace(req.LastName) || string.IsNullOrWhiteSpace(req.Email))
                return BadRequest(new { Message = "Invalid student data." });

            // check group exists
            var group = await _db.AcademicGroups.FindAsync(new object[] { req.GroupId }, ct);
            if (group is null)
                return BadRequest(new { Message = $"Group with ID {req.GroupId} not found." });

            // optional: check subgroup exists when provided
            if (req.SubgroupId.HasValue)
            {
                var sg = await _db.Subgroups.FindAsync(new object[] { req.SubgroupId.Value }, ct);
                if (sg is null) return BadRequest(new { Message = $"Subgroup with ID {req.SubgroupId.Value} not found." });
            }

            // unique email check
            var exists = await _db.Students.AnyAsync(x => x.Email == req.Email, ct);
            if (exists) return BadRequest(new { Message = "Email already in use." });

            var student = new Student
            {
                FirstName = req.FirstName,
                LastName = req.LastName,
                Email = req.Email,
                GroupId = req.GroupId
            };

            _db.Students.Add(student);
            await _db.SaveChangesAsync(ct);

            var dto = new StudentDto(student.Id, student.FirstName, student.LastName, student.Email, student.GroupId, student.SubgroupId);
            return CreatedAtAction(nameof(GetById), new { id = student.Id }, dto);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateStudentRequest req, CancellationToken ct)
        {
            var s = await _db.Students.FindAsync(new object[] { id }, ct);
            if (s is null) return NotFound(new { Message = $"Student with ID {id} not found." });

            // unique email check excluding current
            var exists = await _db.Students.AnyAsync(x => x.Email == req.Email && x.Id != id, ct);
            if (exists) return BadRequest(new { Message = "Email already in use." });

            s.FirstName = req.FirstName;
            s.LastName = req.LastName;
            s.Email = req.Email;
            s.GroupId = req.GroupId;
            s.SubgroupId = req.SubgroupId;

            await _db.SaveChangesAsync(ct);

            return Ok(new StudentDto(s.Id, s.FirstName, s.LastName, s.Email, s.GroupId, s.SubgroupId));
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var s = await _db.Students.FindAsync(new object[] { id }, ct);
            if (s is null) return NotFound(new { Message = $"Student with ID {id} not found." });

            _db.Students.Remove(s);
            await _db.SaveChangesAsync(ct);

            return NoContent();
        }
    }
}
