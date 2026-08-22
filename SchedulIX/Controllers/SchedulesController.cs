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
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Generate([FromBody] GenerateScheduleRequestDto request, CancellationToken ct)
        {
            if (request.GroupIds is null || !request.GroupIds.Any())
            {
                return BadRequest(new { Message = "Trebuie să selectați cel puțin o grupă de studenți." });
            }

            var result = await scheduleService.GenerateScheduleAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
    }
}
