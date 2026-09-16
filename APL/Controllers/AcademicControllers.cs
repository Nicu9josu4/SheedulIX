using AcademicManagement.Data;
using AcademicManagement.Domain;
using AcademicManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicManagement.Controllers;

[ApiController]
[Route("api")]
public class AcademicApiController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly AcademicService _academicService;

    public AcademicApiController(AppDbContext context, AcademicService academicService)
    {
        _context = context;
        _academicService = academicService;
    }

    // === DEPARTAMENTE ===
    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments()
    {
        return Ok(await _context.Departments.Where(d => d.Active).ToListAsync());
    }

    [HttpPost("departments")]
    public async Task<IActionResult> CreateDepartment([FromBody] Department department)
    {
        _context.Departments.Add(department);
        await _context.SaveChangesAsync();
        return Ok(department);
    }

    // === PERSOANE ȘI ANGAJĂRI (CUMUL FĂRĂ DUPLICARE) ===
    [HttpGet("persons/search")]
    public async Task<IActionResult> SearchPersons([FromQuery] string q)
    {
        var persons = await _context.Persons
            .Where(p => p.FirstName.Contains(q) || p.LastName.Contains(q) || p.Email.Contains(q))
            .Take(10)
            .Select(p => new { p.Id, p.FirstName, p.LastName, p.Email, p.AcademicTitle })
            .ToListAsync();
        return Ok(persons);
    }

    [HttpPost("persons")]
    public async Task<IActionResult> CreatePersonWithEmployment([FromBody] CreatePersonDto dto)
    {
        var person = new Person
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Patronymic = dto.Patronymic,
            Email = dto.Email,
            Phone = dto.Phone,
            AcademicTitle = dto.AcademicTitle,
            AcademicDegree = dto.AcademicDegree
        };

        var employment = new DepartmentEmployment
        {
            Person = person,
            DepartmentId = dto.DepartmentId,
            EmploymentType = dto.EmploymentType,
            Position = dto.Position,
            TeachingLoad = dto.TeachingLoad,
            StartDate = DateTime.UtcNow
        };

        var profile = new ProfessorProfile { Person = person };

        _context.Persons.Add(person);
        _context.DepartmentEmployments.Add(employment);
        _context.ProfessorProfiles.Add(profile);

        await _context.SaveChangesAsync();
        return Ok(new { person.Id, person.FirstName, person.LastName });
    }

    [HttpPost("persons/{personId}/employments")]
    public async Task<IActionResult> AddEmploymentForExistingPerson(Guid personId, [FromBody] DepartmentEmployment employment)
    {
        var personExists = await _context.Persons.AnyAsync(p => p.Id == personId);
        if (!personExists) return NotFound("Persoana nu a fost găsită.");

        employment.PersonId = personId;
        employment.StartDate = DateTime.UtcNow;

        _context.DepartmentEmployments.Add(employment);
        await _context.SaveChangesAsync();
        return Ok(employment);
    }

    // === PROFESORI ===
    [HttpGet("professors")]
    public async Task<IActionResult> GetProfessors()
    {
        var professors = await _context.ProfessorProfiles
            .Include(p => p.Person)
                .ThenInclude(p => p.Employments)
                    .ThenInclude(e => e.Department)
            .Include(p => p.Languages)
                .ThenInclude(l => l.Language)
            .Select(p => new
            {
                p.Id,
                PersonId = p.Person.Id,
                FullName = $"{p.Person.LastName} {p.Person.FirstName}",
                p.Person.Email,
                p.Person.AcademicTitle,
                Employments = p.Person.Employments.Select(e => new { e.Department.Name, e.EmploymentType, e.Position }),
                Languages = p.Languages.Select(l => l.Language.Code)
            })
            .ToListAsync();

        return Ok(professors);
    }

    // === GRUPE ȘI SUBGRUPE ===
    [HttpGet("groups")]
    public async Task<IActionResult> GetGroups()
    {
        var groups = await _context.Groups
            .Include(g => g.Subgroups)
            .Include(g => g.Language)
            .ToListAsync();
        return Ok(groups);
    }

    [HttpPost("groups")]
    public async Task<IActionResult> CreateGroup([FromBody] Group group)
    {
        _context.Groups.Add(group);
        await _context.SaveChangesAsync();
        return Ok(group);
    }

    [HttpPost("groups/{groupId}/subgroups/generate")]
    public async Task<IActionResult> GenerateSubgroups(Guid groupId)
    {
        try
        {
            await _academicService.SynchronizeSubgroupsAsync(groupId);
            return Ok(new { Message = "Subgrupele au fost generate cu succes." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    // === SUBJECTS / DISCIPLINE ===
    [HttpGet("subjects")]
    public async Task<IActionResult> GetSubjects()
    {
        return Ok(await _context.Subjects.Where(s => s.Active).ToListAsync());
    }

    [HttpPost("subjects")]
    public async Task<IActionResult> CreateSubject([FromBody] Subject subject)
    {
        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync();
        return Ok(subject);
    }

    // === REPARTIZARE SARCINĂ ===
    [HttpPost("teachingassignments")]
    public async Task<IActionResult> CreateTeachingAssignment([FromBody] TeachingAssignment assignment)
    {
        try
        {
            await _academicService.AssignProfessorAsync(assignment);
            return Ok(assignment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    // === DASHBOARD STATS ===
    [HttpGet("dashboard/stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var stats = new
        {
            TotalEmployees = await _context.Persons.CountAsync(),
            TitulariCount = await _context.DepartmentEmployments.CountAsync(e => e.EmploymentType == EmploymentType.Head),
            CumulInternCount = await _context.DepartmentEmployments.CountAsync(e => e.EmploymentType == EmploymentType.InternalMultiJob),
            CumulExternCount = await _context.DepartmentEmployments.CountAsync(e => e.EmploymentType == EmploymentType.ExternalMultiJob),
            TotalGroups = await _context.Groups.CountAsync(),
            TotalSubgroups = await _context.Subgroups.CountAsync(),
            TotalAssignedHours = await _context.TeachingAssignments.SumAsync(a => a.Hours)
        };
        return Ok(stats);
    }

    [HttpGet("studyprograms")]
    public async Task<IActionResult> GetStudyPrograms()
    {
        return Ok(await _context.StudyPrograms.ToListAsync());
    }

    [HttpGet("curriculums")]
    public async Task<IActionResult> GetCurriculums()
    {
        return Ok(await _context.Curriculums.Where(c => c.IsActive).ToListAsync());
    }

    [HttpGet("languages")]
    public async Task<IActionResult> GetLanguages()
    {
        return Ok(await _context.Languages.ToListAsync());
    }

    [HttpPost("groups/auto-create")]
    public async Task<IActionResult> AutoCreateGroup([FromBody] AutoCreateGroupDto dto)
    {
        // 1. Extrage prefixul programului (ex: din "CR-261FR" extrage "CR")
        var parts = dto.Code.Split('-');
        if (parts.Length < 2)
            return BadRequest("Codul grupei trebuie să fie în formatul CODE-AN (ex: CR-261FR)");

        string programCode = parts[0]; // ex: "CR"
        string yearDigits = parts[1].Substring(0, 2); // ex: "26"

        // 2. Caută Programul de Studii după Cod
        var program = await _context.StudyPrograms
            .FirstOrDefaultAsync(sp => sp.Code == programCode);
        if (program == null)
            return NotFound($"Programul de studii cu codul '{programCode}' nu a fost găsit.");

        // 3. Caută Curriculum-ul aferent anului (ex: "2026" sau "2026-2027")
        var curriculum = await _context.Curriculums
            .FirstOrDefaultAsync(c => c.StudyProgramId == program.Id
                                   && c.Version.Contains($"20{yearDigits}")
                                   && c.IsActive);
        if (curriculum == null)
            return NotFound($"Nu s-a găsit niciun Curriculum activ pentru anul 20{yearDigits} la programul {programCode}.");

        // 4. Creează grupa complet legată
        var group = new Group
        {
            Id = Guid.NewGuid(),
            Code = dto.Code,
            StudyProgramId = program.Id,
            CurriculumId = curriculum.Id,
            LanguageId = dto.LanguageId,
            YearOfStudy = dto.YearOfStudy,
            StudentCount = dto.StudentCount,
        };

        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        // 5. Generare subgrupe dacă depășește 26 studenți (Conform regulilor de business)
        if (group.StudentCount > 26)
        {
            await _academicService.SynchronizeSubgroupsAsync(group.Id);
        }

        return Ok(new
        {
            Message = $"Grupa {group.Code} a fost creată și asociată automat la Programul {program.Name} și Curriculumul {curriculum.Version}",
            Group = group
        });
    }

}


public class AutoCreateGroupDto
{
    public string Code { get; set; } = string.Empty; // ex: CR-261FR
    public Guid LanguageId { get; set; }
    public int YearOfStudy { get; set; } = 1;
    public int StudentCount { get; set; } = 25;
}
public class CreatePersonDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Patronymic { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string AcademicTitle { get; set; } = string.Empty;
    public string AcademicDegree { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public string Position { get; set; } = string.Empty;
    public decimal TeachingLoad { get; set; }
}