using AcademicManagement.Data;
using AcademicManagement.Domain;
using AcademicManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicManagement.Controllers;

/// <summary>
/// Completează API-ul de bază cu operațiile pe care ecranele existente le apelau, dar care lipseau:
/// programe de studii, curriculumuri (cu disciplinele lor), limbi, editări și ștergeri protejate, calificările profesorilor.
/// Ștergerea este refuzată (409) dacă înregistrarea este folosită în altă parte.
/// </summary>
[ApiController]
[Route("api")]
public class CatalogController(AppDbContext context, AcademicService academic) : ControllerBase
{
    private static string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    private IActionResult Bad(string message) => BadRequest(new { Error = message });
    private IActionResult Conflict_(string message) => Conflict(new { Error = message });

    // ===================== Limbi =====================
    [HttpPost("languages")]
    public async Task<IActionResult> CreateLanguage([FromBody] LanguageDto dto)
    {
        var code = Clean(dto.Code)?.ToUpperInvariant(); var name = Clean(dto.Name);
        if (code is null || name is null) return Bad("Codul și numele limbii sunt obligatorii.");
        if (await context.Languages.AnyAsync(l => l.Code == code)) return Conflict_($"Limba cu codul {code} există deja.");
        var lang = new Language { Code = code, Name = name };
        context.Languages.Add(lang);
        await context.SaveChangesAsync();
        return Ok(lang);
    }

    // ===================== Departamente =====================
    [HttpPut("departments/{id:guid}")]
    public async Task<IActionResult> UpdateDepartment(Guid id, [FromBody] DepartmentDto dto)
    {
        var d = await context.Departments.FindAsync(id);
        if (d is null) return NotFound();
        var code = Clean(dto.Code); var name = Clean(dto.Name);
        if (code is null || name is null) return Bad("Codul și numele sunt obligatorii.");
        if (await context.Departments.AnyAsync(x => x.Id != id && x.Code == code)) return Conflict_("Există deja un departament cu acest cod.");
        d.Code = code; d.Name = name; d.Description = Clean(dto.Description) ?? "";
        await context.SaveChangesAsync();
        return Ok(d);
    }

    [HttpDelete("departments/{id:guid}")]
    public async Task<IActionResult> DeleteDepartment(Guid id)
    {
        var d = await context.Departments.FindAsync(id);
        if (d is null) return NotFound();
        if (await context.DepartmentEmployments.AnyAsync(e => e.DepartmentId == id) || await context.StaffingPlans.AnyAsync(p => p.DepartmentId == id))
            return Conflict_("Departamentul are angajări sau state de funcții și nu poate fi șters.");
        context.Departments.Remove(d);
        await context.SaveChangesAsync();
        return NoContent();
    }

    // ===================== Programe de studii =====================
    [HttpPost("studyprograms")]
    public async Task<IActionResult> CreateProgram([FromBody] ProgramDto dto)
    {
        var (error, code, name) = ValidateProgram(dto);
        if (error is not null) return Bad(error);
        if (await context.StudyPrograms.AnyAsync(p => p.Code == code)) return Conflict_($"Există deja un program cu codul {code}.");
        var p = new StudyProgram { Code = code!, Name = name!, DegreeLevel = Clean(dto.DegreeLevel) ?? "Licență", DurationYears = dto.DurationYears };
        context.StudyPrograms.Add(p);
        await context.SaveChangesAsync();
        return Ok(p);
    }

    [HttpPut("studyprograms/{id:guid}")]
    public async Task<IActionResult> UpdateProgram(Guid id, [FromBody] ProgramDto dto)
    {
        var p = await context.StudyPrograms.FindAsync(id);
        if (p is null) return NotFound();
        var (error, code, name) = ValidateProgram(dto);
        if (error is not null) return Bad(error);
        if (await context.StudyPrograms.AnyAsync(x => x.Id != id && x.Code == code)) return Conflict_($"Există deja un program cu codul {code}.");
        p.Code = code!; p.Name = name!; p.DegreeLevel = Clean(dto.DegreeLevel) ?? p.DegreeLevel; p.DurationYears = dto.DurationYears;
        await context.SaveChangesAsync();
        return Ok(p);
    }

    [HttpDelete("studyprograms/{id:guid}")]
    public async Task<IActionResult> DeleteProgram(Guid id)
    {
        var p = await context.StudyPrograms.FindAsync(id);
        if (p is null) return NotFound();
        if (await context.Curriculums.AnyAsync(c => c.StudyProgramId == id) || await context.Groups.AnyAsync(g => g.StudyProgramId == id))
            return Conflict_("Programul are curriculumuri sau grupe și nu poate fi șters.");
        context.StudyPrograms.Remove(p);
        await context.SaveChangesAsync();
        return NoContent();
    }

    private static (string? Error, string? Code, string? Name) ValidateProgram(ProgramDto dto)
    {
        var code = Clean(dto.Code); var name = Clean(dto.Name);
        if (code is null || name is null) return ("Codul și numele programului sunt obligatorii.", null, null);
        if (dto.DurationYears is < 1 or > 8) return ("Durata trebuie să fie între 1 și 8 ani.", null, null);
        return (null, code, name);
    }

    // ===================== Discipline =====================
    [HttpPut("subjects/{id:guid}")]
    public async Task<IActionResult> UpdateSubject(Guid id, [FromBody] SubjectDto dto)
    {
        var s = await context.Subjects.FindAsync(id);
        if (s is null) return NotFound();
        var code = Clean(dto.Code); var name = Clean(dto.Name);
        if (code is null || name is null) return Bad("Codul și numele disciplinei sunt obligatorii.");
        if (dto.Credits < 0 || dto.TotalHours < 0) return Bad("Creditele și orele nu pot fi negative.");
        s.Code = code; s.Name = name; s.Description = Clean(dto.Description) ?? ""; s.Credits = dto.Credits; s.TotalHours = dto.TotalHours;
        await context.SaveChangesAsync();
        return Ok(s);
    }

    [HttpDelete("subjects/{id:guid}")]
    public async Task<IActionResult> DeleteSubject(Guid id)
    {
        var s = await context.Subjects.FindAsync(id);
        if (s is null) return NotFound();
        if (await context.CurriculumSubjects.AnyAsync(x => x.SubjectId == id) || await context.TeachingAssignments.AnyAsync(a => a.SubjectId == id))
            return Conflict_("Disciplina este folosită în curriculumuri sau repartizări și nu poate fi ștearsă.");
        context.Set<ProfessorSubject>().RemoveRange(context.Set<ProfessorSubject>().Where(ps => ps.SubjectId == id));
        context.Subjects.Remove(s);
        await context.SaveChangesAsync();
        return NoContent();
    }

    // ===================== Curriculumuri =====================
    [HttpPost("curriculums")]
    public async Task<IActionResult> CreateCurriculum([FromBody] CurriculumDto dto)
    {
        var version = Clean(dto.Version);
        var error = await ValidateCurriculumAsync(dto, version, null);
        if (error is not null) return Bad(error);
        if (await context.Curriculums.AnyAsync(c => c.StudyProgramId == dto.StudyProgramId && c.Version == version))
            return Conflict_($"Programul are deja un curriculum cu versiunea {version}.");
        var c = new Curriculum { StudyProgramId = dto.StudyProgramId, Version = version!, IsActive = dto.IsActive ?? true, CreatedAt = DateTime.UtcNow };
        context.Curriculums.Add(c);
        await context.SaveChangesAsync();
        return Ok(new { c.Id, c.StudyProgramId, c.Version, c.IsActive, c.CreatedAt });
    }

    [HttpPut("curriculums/{id:guid}")]
    public async Task<IActionResult> UpdateCurriculum(Guid id, [FromBody] CurriculumDto dto)
    {
        var c = await context.Curriculums.FindAsync(id);
        if (c is null) return NotFound();
        var version = Clean(dto.Version);
        var error = await ValidateCurriculumAsync(dto, version, id);
        if (error is not null) return Bad(error);
        if (await context.Curriculums.AnyAsync(x => x.Id != id && x.StudyProgramId == dto.StudyProgramId && x.Version == version))
            return Conflict_($"Programul are deja un curriculum cu versiunea {version}.");
        if (c.StudyProgramId != dto.StudyProgramId && await context.Groups.AnyAsync(g => g.CurriculumId == id))
            return Conflict_("Curriculumul are grupe asociate: nu îi poți schimba programul.");
        c.StudyProgramId = dto.StudyProgramId; c.Version = version!; c.IsActive = dto.IsActive ?? c.IsActive;
        await context.SaveChangesAsync();
        return Ok(new { c.Id, c.StudyProgramId, c.Version, c.IsActive, c.CreatedAt });
    }

    [HttpDelete("curriculums/{id:guid}")]
    public async Task<IActionResult> DeleteCurriculum(Guid id)
    {
        var c = await context.Curriculums.FindAsync(id);
        if (c is null) return NotFound();
        if (await context.Groups.AnyAsync(g => g.CurriculumId == id))
            return Conflict_("Curriculumul are grupe asociate și nu poate fi șters. Poți să-l dezactivezi.");
        context.CurriculumSubjects.RemoveRange(context.CurriculumSubjects.Where(x => x.CurriculumId == id));
        context.Curriculums.Remove(c);
        await context.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string?> ValidateCurriculumAsync(CurriculumDto dto, string? version, Guid? id)
    {
        if (!await context.StudyPrograms.AnyAsync(p => p.Id == dto.StudyProgramId)) return "Programul de studii nu există.";
        if (version is null) return "Versiunea este obligatorie (ex. 2026-2027).";
        try { WorkloadService.AcademicYearRange(version); }
        catch (ArgumentException) { return "Versiunea trebuie să fie anul de intrare în formatul 2026-2027."; }
        return null;
    }

    [HttpPost("curriculums/{id:guid}/subjects")]
    public async Task<IActionResult> AddCurriculumSubject(Guid id, [FromBody] CurriculumSubjectDto dto)
    {
        if (!await context.Curriculums.AnyAsync(c => c.Id == id)) return NotFound(new { Error = "Curriculumul nu există." });
        if (!await context.Subjects.AnyAsync(s => s.Id == dto.SubjectId)) return Bad("Disciplina nu există.");
        if (!await context.Languages.AnyAsync(l => l.Id == dto.LanguageId)) return Bad("Limba nu există.");
        if (dto.YearOfStudy is < 1 or > 8) return Bad("Anul de studiu trebuie să fie între 1 și 8.");
        if (dto.Semester is < 1 or > 2) return Bad("Semestrul trebuie să fie 1 sau 2.");
        if (dto.LectureHours < 0 || dto.SeminarHours < 0 || dto.LabHours < 0) return Bad("Orele nu pot fi negative.");
        if (dto.LectureHours + dto.SeminarHours + dto.LabHours == 0) return Bad("Cel puțin un tip de oră trebuie să fie mai mare ca 0.");
        if (await context.CurriculumSubjects.AnyAsync(x => x.CurriculumId == id && x.SubjectId == dto.SubjectId && x.LanguageId == dto.LanguageId))
            return Conflict_("Disciplina cu această limbă există deja în curriculum.");

        var cs = new CurriculumSubject
        {
            CurriculumId = id, SubjectId = dto.SubjectId, LanguageId = dto.LanguageId, YearOfStudy = dto.YearOfStudy,
            Semester = dto.Semester, LectureHours = dto.LectureHours, SeminarHours = dto.SeminarHours, LabHours = dto.LabHours
        };
        context.CurriculumSubjects.Add(cs);
        await context.SaveChangesAsync();
        return Ok(new { cs.Id });
    }

    [HttpDelete("curriculums/{id:guid}/subjects/{entryId:guid}")]
    public async Task<IActionResult> DeleteCurriculumSubject(Guid id, Guid entryId)
    {
        var cs = await context.CurriculumSubjects.FirstOrDefaultAsync(x => x.Id == entryId && x.CurriculumId == id);
        if (cs is null) return NotFound();
        context.CurriculumSubjects.Remove(cs);
        await context.SaveChangesAsync();
        return NoContent();
    }

    // ===================== Persoane =====================
    [HttpPut("persons/{id:guid}")]
    public async Task<IActionResult> UpdatePerson(Guid id, [FromBody] PersonUpdateDto dto)
    {
        var p = await context.Persons.Include(x => x.Employments).FirstOrDefaultAsync(x => x.Id == id);
        if (p is null) return NotFound();
        var first = Clean(dto.FirstName); var last = Clean(dto.LastName);
        if (first is null || last is null) return Bad("Prenumele și numele sunt obligatorii.");
        var email = Clean(dto.Email) ?? "";
        if (email != "" && !(email.Contains('@') && email.Contains('.'))) return Bad("Email invalid.");
        if (email != "" && await context.Persons.AnyAsync(x => x.Id != id && x.Email.ToLower() == email.ToLower()))
            return Conflict_("Există deja o persoană cu acest email.");

        p.FirstName = first; p.LastName = last; p.Patronymic = Clean(dto.Patronymic); p.Email = email;
        p.Phone = Clean(dto.Phone) ?? p.Phone; p.AcademicTitle = Clean(dto.AcademicTitle) ?? p.AcademicTitle;
        p.AcademicDegree = Clean(dto.AcademicDegree) ?? p.AcademicDegree;

        // angajarea din departamentul indicat (dacă există) se actualizează
        if (dto.DepartmentId is { } dep && p.Employments.FirstOrDefault(e => e.DepartmentId == dep && e.Active) is { } emp)
        {
            if (dto.EmploymentType is { } t) emp.EmploymentType = t;
            if (Clean(dto.Position) is { } pos) emp.Position = pos;
            if (dto.TeachingLoad is { } load && load >= 0) emp.TeachingLoad = load;
        }
        await context.SaveChangesAsync();
        return Ok(new { p.Id, p.FirstName, p.LastName, p.Email });
    }

    // ===================== Grupe =====================
    [HttpPut("groups/{id:guid}")]
    public async Task<IActionResult> UpdateGroup(Guid id, [FromBody] GroupDto dto)
    {
        var g = await context.Groups.Include(x => x.Subgroups).FirstOrDefaultAsync(x => x.Id == id);
        if (g is null) return NotFound();
        var code = Clean(dto.Code);
        if (code is null) return Bad("Codul grupei este obligatoriu.");
        if (dto.StudentCount < 1) return Bad("Numărul de studenți trebuie să fie pozitiv.");
        if (dto.YearOfStudy is < 1 or > 8) return Bad("Anul de studiu trebuie să fie între 1 și 8.");
        if (await context.Groups.AnyAsync(x => x.Id != id && x.Code == code)) return Conflict_("Există deja o grupă cu acest cod.");
        var curriculum = await context.Curriculums.FirstOrDefaultAsync(c => c.Id == dto.CurriculumId && c.StudyProgramId == dto.StudyProgramId);
        if (curriculum is null) return Bad("Curriculumul trebuie să aparțină programului ales.");
        if (!await context.Languages.AnyAsync(l => l.Id == dto.LanguageId)) return Bad("Limba nu există.");

        // subgrupele depind de numărul de studenți (> 26); nu le recreăm dacă au repartizări
        bool subgroupsChange = (g.StudentCount > 26) != (dto.StudentCount > 26) || (dto.StudentCount > 26 && g.StudentCount != dto.StudentCount);
        if (subgroupsChange && g.Subgroups.Count > 0 &&
            await context.TeachingAssignments.AnyAsync(a => a.SubgroupId != null && a.GroupId == id))
            return Conflict_("Grupa are repartizări pe subgrupe: numărul de studenți nu poate schimba subgrupele.");

        g.Code = code; g.StudyProgramId = dto.StudyProgramId; g.CurriculumId = dto.CurriculumId; g.LanguageId = dto.LanguageId;
        g.YearOfStudy = dto.YearOfStudy; g.StudentCount = dto.StudentCount;
        await context.SaveChangesAsync();
        if (subgroupsChange) await academic.SynchronizeSubgroupsAsync(id);
        return Ok(new { g.Id, g.Code });
    }

    // ===================== Calificări =====================
    [HttpGet("professors/{professorId:guid}/qualifications")]
    public async Task<IActionResult> GetQualifications(Guid professorId)
    {
        if (!await context.ProfessorProfiles.AnyAsync(p => p.Id == professorId)) return NotFound(new { Error = "Profesorul nu a fost găsit." });
        return Ok(new QualificationsDto(
            await context.Set<ProfessorSubject>().Where(x => x.ProfessorProfileId == professorId).Select(x => x.SubjectId).ToListAsync(),
            await context.Set<ProfessorLanguage>().Where(x => x.ProfessorProfileId == professorId).Select(x => x.LanguageId).ToListAsync()));
    }

    /// <summary>Înlocuiește calificările profesorului (disciplinele și limbile pe care le poate preda).</summary>
    [HttpPut("professors/{professorId:guid}/qualifications")]
    public async Task<IActionResult> SaveQualifications(Guid professorId, [FromBody] QualificationsDto dto)
    {
        if (!await context.ProfessorProfiles.AnyAsync(p => p.Id == professorId)) return NotFound(new { Error = "Profesorul nu a fost găsit." });
        var subjectIds = (dto.SubjectIds ?? []).Distinct().ToList();
        var languageIds = (dto.LanguageIds ?? []).Distinct().ToList();
        if (await context.Subjects.CountAsync(s => subjectIds.Contains(s.Id)) != subjectIds.Count) return Bad("O disciplină din listă nu există.");
        if (await context.Languages.CountAsync(l => languageIds.Contains(l.Id)) != languageIds.Count) return Bad("O limbă din listă nu există.");

        await using var tx = await context.Database.BeginTransactionAsync();
        context.Set<ProfessorSubject>().RemoveRange(context.Set<ProfessorSubject>().Where(x => x.ProfessorProfileId == professorId));
        context.Set<ProfessorLanguage>().RemoveRange(context.Set<ProfessorLanguage>().Where(x => x.ProfessorProfileId == professorId));
        await context.SaveChangesAsync();
        context.Set<ProfessorSubject>().AddRange(subjectIds.Select(s => new ProfessorSubject { ProfessorProfileId = professorId, SubjectId = s }));
        context.Set<ProfessorLanguage>().AddRange(languageIds.Select(l => new ProfessorLanguage { ProfessorProfileId = professorId, LanguageId = l }));
        await context.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(new { Saved = true, Subjects = subjectIds.Count, Languages = languageIds.Count });
    }
}

public record LanguageDto(string Code, string Name);
public record DepartmentDto(string Code, string Name, string? Description);
public record ProgramDto(string Code, string Name, string? DegreeLevel, int DurationYears);
public record SubjectDto(string Code, string Name, string? Description, int Credits, int TotalHours);
public record CurriculumDto(Guid StudyProgramId, string Version, bool? IsActive);
public record CurriculumSubjectDto(Guid SubjectId, Guid LanguageId, int YearOfStudy, int Semester, int LectureHours, int SeminarHours, int LabHours);
public record PersonUpdateDto(string FirstName, string LastName, string? Patronymic, string? Email, string? Phone,
    string? AcademicTitle, string? AcademicDegree, Guid? DepartmentId, EmploymentType? EmploymentType, string? Position, decimal? TeachingLoad);
public record GroupDto(string Code, Guid StudyProgramId, Guid CurriculumId, Guid LanguageId, int YearOfStudy, int StudentCount);
public record QualificationsDto(List<Guid> SubjectIds, List<Guid> LanguageIds);
