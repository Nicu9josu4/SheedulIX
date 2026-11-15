using AcademicManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace AcademicManagement.Controllers;

[ApiController]
[Route("api/import")]
public class ImportController(ImportService import) : ControllerBase
{
    private const long MaxBytes = 5 * 1024 * 1024;

    /// <summary>Validează fișierul fără să salveze nimic și întoarce raportul pe foi și rânduri.</summary>
    [HttpPost("preview")]
    [RequestSizeLimit(MaxBytes)]
    public Task<IActionResult> Preview(IFormFile file) => Run(file, commit: false);

    /// <summary>Salvează datele, doar dacă fișierul nu are nicio eroare (totul sau nimic).</summary>
    [HttpPost("commit")]
    [RequestSizeLimit(MaxBytes)]
    public Task<IActionResult> Commit(IFormFile file) => Run(file, commit: true);

    private async Task<IActionResult> Run(IFormFile? file, bool commit)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { Error = "Alege un fișier .xlsx." });
        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { Error = "Sunt acceptate doar fișiere .xlsx (șablonul de import)." });
        if (file.Length > MaxBytes)
            return BadRequest(new { Error = "Fișierul depășește 5 MB." });

        try
        {
            await using var stream = file.OpenReadStream();
            var report = await import.RunAsync(stream, commit);
            if (report.Sheets.Count == 0)
                return BadRequest(new { Error = "Fișierul nu conține nicio foaie cunoscută (Departamente, Cadre_didactice, Discipline). Folosește șablonul." });
            return Ok(report);
        }
        catch (Exception ex) when (ex is InvalidDataException or System.IO.IOException or ArgumentException)
        {
            return BadRequest(new { Error = "Fișierul nu poate fi citit ca .xlsx: " + ex.Message });
        }
    }
}
