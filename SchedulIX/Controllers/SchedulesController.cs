using Microsoft.AspNetCore.Mvc;
using SchedulIX.Interfaces;
using SchedulIX.Models;

namespace SchedulIX.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchedulesController(IScheduleService scheduleService) : ControllerBase
    {

        /// <summary>
        /// Obține detaliile unui orar după ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ScheduleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var schedule = await scheduleService.GetByIdAsync(id, ct);
            if (schedule is null)
                return NotFound(new { Message = $"Orarul cu ID-ul {id} nu a fost găsit." });

            return Ok(schedule);
        }

        /// <summary>
        /// Declanșează algoritmul automat de generare a orarului
        /// </summary>
        [HttpPost("generate")]
        [ProducesResponseType(typeof(ScheduleDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> Generate([FromBody] GenerateScheduleRequestDto request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request.GroupIds is null || !request.GroupIds.Any())
            {
                return BadRequest(new { Message = "Trebuie să selectați cel puțin o grupă de studenți." });
            }

            var result = await scheduleService.GenerateScheduleAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Validează o programare (verifi dacă respectă regulile)
        /// </summary>
        [HttpPost("validate")]
        [ProducesResponseType(typeof(ValidationResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Validate([FromBody] ScheduleDto schedule, CancellationToken ct)
        {
            var result = await scheduleService.ValidateScheduleAsync(schedule, ct);
            return Ok(result);
        }
    }

    public record ValidationResultDto(
        bool IsValid,
        List<string> Violations,
        int HardConstraintViolations,
        int SoftConstraintScore
    );
}
