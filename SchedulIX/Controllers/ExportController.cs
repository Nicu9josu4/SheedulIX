using Microsoft.AspNetCore.Mvc;
using SchedulIX.Services.Interfaces;

namespace SchedulIX.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExportController : ControllerBase
    {
        private readonly IExportService _exportService;

        public ExportController(IExportService exportService)
        {
            _exportService = exportService;
        }

        /// <summary>
        /// Exportă orarul în format Excel (.xlsx)
        /// </summary>
        [HttpGet("excel")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ExportToExcel(CancellationToken ct)
        {
            try
            {
                var stream = await _exportService.ExportToExcelAsync(ct);
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    $"Orar_{DateTime.Now:yyyy-MM-dd_HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = $"Eroare la export: {ex.Message}" });
            }
        }

        /// <summary>
        /// Exportă orarul în format PDF
        /// </summary>
        [HttpGet("pdf")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ExportToPdf(CancellationToken ct)
        {
            try
            {
                var stream = await _exportService.ExportToPdfAsync(ct);
                return File(stream, "application/pdf", $"Orar_{DateTime.Now:yyyy-MM-dd_HHmmss}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = $"Eroare la export: {ex.Message}" });
            }
        }

        /// <summary>
        /// Exportă calendarul unei săli specifice în Excel
        /// </summary>
        [HttpGet("room/{roomId:int}/excel")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ExportRoomScheduleToExcel(int roomId, CancellationToken ct)
        {
            try
            {
                var stream = await _exportService.ExportRoomScheduleToExcelAsync(roomId, ct);
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    $"Orar_Sala_{roomId}_{DateTime.Now:yyyy-MM-dd}.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = $"Eroare la export: {ex.Message}" });
            }
        }
    }
}
