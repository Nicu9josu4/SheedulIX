using AcademicManagement.Data;
using AcademicManagement.Domain;
using AcademicManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicManagement.Controllers;

[ApiController]
[Route("api")]
public class WorkloadController(AppDbContext context, WorkloadService workload, StaffingPlanService plans) : ControllerBase
{
    // ===== Configurare: norme si coeficienti =====
    [HttpGet("position-norms")]
    public async Task<IActionResult> GetNorms([FromQuery] string? academicYear) =>
        Ok(await context.PositionNorms
            .Where(n => academicYear == null || n.AcademicYear == academicYear)
            .OrderBy(n => n.AcademicYear).ThenBy(n => n.Position).ToListAsync());

    [HttpPost("position-norms")]
    public async Task<IActionResult> CreateNorm([FromBody] PositionNorm norm)
    {
        if (string.IsNullOrWhiteSpace(norm.Position)) return BadRequest(new { Error = "Funcția este obligatorie." });
        if (norm.MinHours > norm.BaseNormHours || norm.BaseNormHours > norm.MaxHours)
            return BadRequest(new { Error = "Trebuie respectat: minim ≤ normă de bază ≤ maxim." });
        try { WorkloadService.AcademicYearRange(norm.AcademicYear); }
        catch (ArgumentException ex) { return BadRequest(new { Error = ex.Message }); }
        if (await context.PositionNorms.AnyAsync(n => n.Position == norm.Position && n.AcademicYear == norm.AcademicYear))
            return Conflict(new { Error = "Norma pentru această funcție și an există deja." });

        context.PositionNorms.Add(norm);
        await context.SaveChangesAsync();
        return Ok(norm);
    }

    [HttpGet("conversion-coefficients")]
    public async Task<IActionResult> GetCoefficients([FromQuery] string? academicYear) =>
        Ok(await context.ConversionCoefficients
            .Where(c => academicYear == null || c.AcademicYear == academicYear)
            .OrderBy(c => c.AcademicYear).ThenBy(c => c.ActivityType).ToListAsync());

    [HttpPost("conversion-coefficients")]
    public async Task<IActionResult> CreateCoefficient([FromBody] ConversionCoefficient coefficient)
    {
        if (coefficient.Coefficient <= 0) return BadRequest(new { Error = "Coeficientul trebuie să fie pozitiv." });
        try { WorkloadService.AcademicYearRange(coefficient.AcademicYear); }
        catch (ArgumentException ex) { return BadRequest(new { Error = ex.Message }); }
        if (await context.ConversionCoefficients.AnyAsync(c =>
                c.AcademicYear == coefficient.AcademicYear && c.ActivityType == coefficient.ActivityType))
            return Conflict(new { Error = "Coeficientul pentru acest tip de activitate și an există deja." });

        context.ConversionCoefficients.Add(coefficient);
        await context.SaveChangesAsync();
        return Ok(coefficient);
    }

    // ===== Calculul sarcinii =====
    [HttpGet("professors/{professorProfileId:guid}/workload")]
    public async Task<IActionResult> GetWorkload(Guid professorProfileId, [FromQuery] string academicYear, [FromQuery] int semester = 1)
    {
        try { return Ok(await workload.CalculateAsync(professorProfileId, academicYear, semester)); }
        catch (KeyNotFoundException ex) { return NotFound(new { Error = ex.Message }); }
    }

    [HttpGet("departments/{departmentId:guid}/workload")]
    public async Task<IActionResult> GetDepartmentWorkload(Guid departmentId, [FromQuery] string academicYear)
    {
        try { return Ok(await plans.DepartmentWorkloadAsync(departmentId, academicYear)); }
        catch (ArgumentException ex) { return BadRequest(new { Error = ex.Message }); }
    }

    // ===== Cumul: contracte =====
    [HttpGet("persons/{personId:guid}/contracts")]
    public async Task<IActionResult> GetContracts(Guid personId) =>
        Ok(await context.Contracts
            .Where(c => c.DepartmentEmployment.PersonId == personId)
            .OrderByDescending(c => c.StartDate).ToListAsync());

    [HttpPost("employments/{employmentId:guid}/contracts")]
    public async Task<IActionResult> CreateContract(Guid employmentId, [FromBody] Contract contract)
    {
        var employment = await context.DepartmentEmployments.FirstOrDefaultAsync(e => e.Id == employmentId);
        if (employment is null) return NotFound(new { Error = "Angajarea nu a fost găsită." });
        if (contract.EndDate <= contract.StartDate)
            return BadRequest(new { Error = "Data de sfârșit trebuie să fie după data de început." });
        if (contract.WeeklyHours <= 0)
            return BadRequest(new { Error = "Orele pe săptămână trebuie să fie pozitive." });
        if (string.IsNullOrWhiteSpace(contract.Number))
            return BadRequest(new { Error = "Numărul contractului este obligatoriu." });
        if (employment.EmploymentType == EmploymentType.ExternalMultiJob && string.IsNullOrWhiteSpace(contract.BaseEmployer))
            return BadRequest(new { Error = "Pentru cumul extern trebuie specificat angajatorul de bază." });
        if (await context.Contracts.AnyAsync(c => c.Number == contract.Number))
            return Conflict(new { Error = "Există deja un contract cu acest număr." });

        contract.DepartmentEmploymentId = employmentId;
        contract.StartDate = DateTime.SpecifyKind(contract.StartDate, DateTimeKind.Utc);
        contract.EndDate = DateTime.SpecifyKind(contract.EndDate, DateTimeKind.Utc);
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();
        return Ok(contract);
    }

    [HttpGet("contracts/expiring")]
    public async Task<IActionResult> GetExpiringContracts([FromQuery] int days = 30)
    {
        var limit = DateTime.UtcNow.AddDays(days);
        return Ok(await context.Contracts
            .Where(c => c.Status == ContractStatus.Active && c.EndDate <= limit)
            .OrderBy(c => c.EndDate)
            .Select(c => new
            {
                c.Id, c.Number, c.EndDate,
                Person = c.DepartmentEmployment.Person.LastName + " " + c.DepartmentEmployment.Person.FirstName,
                Department = c.DepartmentEmployment.Department.Name
            }).ToListAsync());
    }

    [HttpPost("contracts/{contractId:guid}/status")]
    public async Task<IActionResult> SetContractStatus(Guid contractId, [FromBody] ContractStatus status)
    {
        var contract = await context.Contracts.FindAsync(contractId);
        if (contract is null) return NotFound();
        contract.Status = status;
        await context.SaveChangesAsync();
        return Ok(contract);
    }

    // ===== Cumul: documente =====
    [HttpGet("persons/{personId:guid}/documents")]
    public async Task<IActionResult> GetDocuments(Guid personId) =>
        Ok(await context.EmploymentDocuments.Where(d => d.PersonId == personId).ToListAsync());

    [HttpPost("persons/{personId:guid}/documents")]
    public async Task<IActionResult> AddDocument(Guid personId, [FromBody] EmploymentDocument document)
    {
        if (!await context.Persons.AnyAsync(p => p.Id == personId))
            return NotFound(new { Error = "Persoana nu a fost găsită." });
        if (document.ContractId.HasValue && !await context.Contracts.AnyAsync(c => c.Id == document.ContractId))
            return BadRequest(new { Error = "Contractul specificat nu există." });

        document.PersonId = personId;
        document.Status = string.IsNullOrWhiteSpace(document.FileName) ? DocumentStatus.Missing : DocumentStatus.Submitted;
        document.UploadedAt = document.Status == DocumentStatus.Submitted ? DateTime.UtcNow : null;
        document.ValidatedAt = null; document.ValidatedBy = null;
        context.EmploymentDocuments.Add(document);
        await context.SaveChangesAsync();
        return Ok(document);
    }

    [HttpPost("documents/{documentId:guid}/review")]
    public async Task<IActionResult> ReviewDocument(Guid documentId, [FromBody] ReviewDocumentDto dto)
    {
        var document = await context.EmploymentDocuments.FindAsync(documentId);
        if (document is null) return NotFound();
        if (document.Status != DocumentStatus.Submitted)
            return BadRequest(new { Error = "Doar documentele depuse pot fi validate sau respinse." });
        if (dto.Status is not (DocumentStatus.Validated or DocumentStatus.Rejected))
            return BadRequest(new { Error = "Statusul trebuie să fie Validated sau Rejected." });
        if (string.IsNullOrWhiteSpace(dto.Reviewer))
            return BadRequest(new { Error = "Trebuie specificat cine verifică documentul." });

        document.Status = dto.Status;
        document.ValidatedAt = DateTime.UtcNow;
        document.ValidatedBy = dto.Reviewer;
        await context.SaveChangesAsync();
        return Ok(document);
    }

    // ===== Stat de functii + aprobare =====
    [HttpPost("staffing-plans")]
    public async Task<IActionResult> CreatePlan([FromBody] CreatePlanDto dto)
    {
        try { return Ok(await plans.CreateAsync(dto.DepartmentId, dto.AcademicYear)); }
        catch (KeyNotFoundException ex) { return NotFound(new { Error = ex.Message }); }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        { return BadRequest(new { Error = ex.Message }); }
    }

    [HttpGet("staffing-plans/{planId:guid}")]
    public async Task<IActionResult> GetPlan(Guid planId)
    {
        var plan = await context.StaffingPlans
            .Include(p => p.Department).Include(p => p.ApprovalSteps.OrderBy(s => s.CreatedAt))
            .FirstOrDefaultAsync(p => p.Id == planId);
        return plan is null ? NotFound() : Ok(plan);
    }

    [HttpPost("staffing-plans/{planId:guid}/transition")]
    public async Task<IActionResult> Transition(Guid planId, [FromBody] TransitionDto dto)
    {
        try { return Ok(await plans.TransitionAsync(planId, dto.ToStatus, dto.Actor, dto.Comment)); }
        catch (KeyNotFoundException ex) { return NotFound(new { Error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { Error = ex.Message }); }
    }
}

public record ReviewDocumentDto(DocumentStatus Status, string Reviewer);
public record CreatePlanDto(Guid DepartmentId, string AcademicYear);
public record TransitionDto(StaffingPlanStatus ToStatus, string Actor, string? Comment);
