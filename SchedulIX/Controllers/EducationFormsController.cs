using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchedulIX.Data;
using SchedulIX.Models.DTOs;

namespace SchedulIX.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EducationFormsController : ControllerBase
    {
        private readonly ScheduleDbContext _db;

        public EducationFormsController(ScheduleDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<EducationFormDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var list = await _db.EducationForms
                .AsNoTracking()
                .Select(e => new EducationFormDto(e.Id, e.Name))
                .ToListAsync(ct);

            return Ok(list);
        }
    }
}
