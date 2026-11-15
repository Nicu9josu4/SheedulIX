using AcademicManagement.Data;
using AcademicManagement.Domain;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace AcademicManagement.Services;

public record ImportRowError(int Row, string Message);

public class SheetReport
{
    public string Sheet { get; set; } = "";
    public int Created { get; set; }
    public int Updated { get; set; }
    public int Unchanged { get; set; }
    public List<ImportRowError> Errors { get; } = new();
}

public class ImportReport
{
    public bool Committed { get; set; }
    public List<SheetReport> Sheets { get; } = new();
    public List<string> Notes { get; } = new();
    public int TotalErrors => Sheets.Sum(s => s.Errors.Count);
}

/// <summary>
/// Import din Excel (șablonul UTM_sablon_import.xlsx): Departamente, Cadre_didactice, Discipline.
/// Preview și commit folosesc exact aceeași logică: totul rulează într-o tranzacție, care la preview
/// (sau dacă există erori) se anulează. Importul este idempotent: cheia existentă se actualizează.
/// </summary>
public class ImportService(AppDbContext context)
{
    private const int HeaderRow = 2, FirstDataRow = 4;

    public async Task<ImportReport> RunAsync(Stream xlsx, bool commit)
    {
        var report = new ImportReport();
        using var wb = new XLWorkbook(xlsx);

        await using var tx = await context.Database.BeginTransactionAsync();
        try
        {
            if (wb.TryGetWorksheet("Departamente", out var wsD)) await ImportDepartmentsAsync(wsD, report);
            else report.Notes.Add("Foaia „Departamente” lipsește — sărită.");

            if (wb.TryGetWorksheet("Cadre_didactice", out var wsP)) await ImportPeopleAsync(wsP, report);
            else report.Notes.Add("Foaia „Cadre_didactice” lipsește — sărită.");

            if (wb.TryGetWorksheet("Discipline", out var wsS)) await ImportSubjectsAsync(wsS, report);
            else report.Notes.Add("Foaia „Discipline” lipsește — sărită.");

            if (commit && report.TotalErrors == 0)
            {
                await tx.CommitAsync();
                report.Committed = true;
            }
            else
            {
                await tx.RollbackAsync();
                if (commit) report.Notes.Add("Nimic nu a fost salvat: corectează erorile și încarcă din nou.");
            }
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
        finally
        {
            context.ChangeTracker.Clear();
        }
        return report;
    }

    // ===================== Departamente =====================
    private async Task ImportDepartmentsAsync(IXLWorksheet ws, ImportReport report)
    {
        var sr = new SheetReport { Sheet = "Departamente" };
        report.Sheets.Add(sr);
        var cols = Columns(ws);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in DataRows(ws, cols))
        {
            var code = Text(row, cols, "Cod");
            var name = Text(row, cols, "Denumire");
            var desc = Text(row, cols, "Descriere") ?? "";
            int r = row.RowNumber();

            if (code is null) { sr.Errors.Add(new(r, "Codul este obligatoriu.")); continue; }
            if (name is null) { sr.Errors.Add(new(r, "Denumirea este obligatorie.")); continue; }
            if (!seen.Add(code)) { sr.Errors.Add(new(r, $"Codul „{code}” apare de mai multe ori în foaie.")); continue; }

            var existing = await context.Departments.FirstOrDefaultAsync(d => d.Code.ToLower() == code.ToLower());
            if (existing is null)
            {
                context.Departments.Add(new Department { Code = code, Name = name, Description = desc });
                sr.Created++;
            }
            else if (existing.Name != name || (desc != "" && existing.Description != desc))
            {
                existing.Name = name;
                if (desc != "") existing.Description = desc;
                sr.Updated++;
            }
            else sr.Unchanged++;
            await context.SaveChangesAsync();
        }
    }

    // ===================== Cadre didactice =====================
    private static readonly Dictionary<string, EmploymentType> EmploymentMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Titular"] = EmploymentType.Head,
        ["CumulIntern"] = EmploymentType.InternalMultiJob,
        ["CumulExtern"] = EmploymentType.ExternalMultiJob,
    };

    private async Task ImportPeopleAsync(IXLWorksheet ws, ImportReport report)
    {
        var sr = new SheetReport { Sheet = "Cadre_didactice" };
        report.Sheets.Add(sr);
        var cols = Columns(ws);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in DataRows(ws, cols))
        {
            int r = row.RowNumber();
            var first = Text(row, cols, "Prenume");
            var last = Text(row, cols, "Nume");
            var email = Text(row, cols, "Email") ?? "";
            var phone = Text(row, cols, "Telefon") ?? "";
            var title = Text(row, cols, "Titlu didactic") ?? "";
            var degree = Text(row, cols, "Grad științific") ?? "";
            var deptCode = Text(row, cols, "Cod departament");
            var typeText = Text(row, cols, "Tip angajare");
            var position = Text(row, cols, "Funcție (pentru normă)");

            if (first is null || last is null) { sr.Errors.Add(new(r, "Prenumele și numele sunt obligatorii.")); continue; }
            if (deptCode is null) { sr.Errors.Add(new(r, "Codul departamentului este obligatoriu.")); continue; }
            if (typeText is null || !EmploymentMap.TryGetValue(typeText, out var type))
            { sr.Errors.Add(new(r, $"Tip angajare invalid „{typeText}”. Valori permise: Titular, CumulIntern, CumulExtern.")); continue; }
            if (position is null) { sr.Errors.Add(new(r, "Funcția este obligatorie (trebuie să coincidă cu o funcție din modelul de calcul).")); continue; }
            if (email != "" && !(email.Contains('@') && email.Contains('.')))
            { sr.Errors.Add(new(r, $"Email invalid: „{email}”.")); continue; }

            decimal load = 0;
            var loadCell = row.Cell(cols["Încărcare (fracție)"]);
            if (!loadCell.IsEmpty() && !loadCell.TryGetValue(out load))
            { sr.Errors.Add(new(r, "Încărcarea trebuie să fie un număr (ex. 1 sau 0.5).")); continue; }
            if (load < 0 || load > 2) { sr.Errors.Add(new(r, "Încărcarea trebuie să fie între 0 și 2.")); continue; }

            var dept = await context.Departments.FirstOrDefaultAsync(d => d.Code.ToLower() == deptCode.ToLower());
            if (dept is null)
            { sr.Errors.Add(new(r, $"Departamentul „{deptCode}” nu există (adaugă-l în foaia Departamente).")); continue; }

            // persoana: dupa email, altfel dupa nume + prenume
            var personKey = email != "" ? email : $"{last}|{first}";
            if (!seen.Add(personKey + "|" + dept.Code))
            { sr.Errors.Add(new(r, "Aceeași persoană apare de mai multe ori în același departament.")); continue; }

            Person? person = email != ""
                ? await context.Persons.Include(p => p.Employments).FirstOrDefaultAsync(p => p.Email.ToLower() == email.ToLower())
                : await context.Persons.Include(p => p.Employments)
                    .FirstOrDefaultAsync(p => p.LastName.ToLower() == last.ToLower() && p.FirstName.ToLower() == first.ToLower());

            bool changed = false, createdPerson = false;
            if (person is null)
            {
                person = new Person
                {
                    FirstName = first, LastName = last, Email = email, Phone = phone,
                    AcademicTitle = title, AcademicDegree = degree
                };
                context.Persons.Add(person);
                context.ProfessorProfiles.Add(new ProfessorProfile { Person = person });
                createdPerson = true;
            }
            else
            {
                changed |= Set(() => person.FirstName, v => person.FirstName = v, first);
                changed |= Set(() => person.LastName, v => person.LastName = v, last);
                if (email != "") changed |= Set(() => person.Email, v => person.Email = v, email);
                if (phone != "") changed |= Set(() => person.Phone, v => person.Phone = v, phone);
                if (title != "") changed |= Set(() => person.AcademicTitle, v => person.AcademicTitle = v, title);
                if (degree != "") changed |= Set(() => person.AcademicDegree, v => person.AcademicDegree = v, degree);
                if (!await context.ProfessorProfiles.AnyAsync(p => p.PersonId == person.Id))
                { context.ProfessorProfiles.Add(new ProfessorProfile { PersonId = person.Id }); changed = true; }
            }

            var employment = person.Employments.FirstOrDefault(e => e.DepartmentId == dept.Id && e.Active);
            if (employment is null)
            {
                context.DepartmentEmployments.Add(new DepartmentEmployment
                {
                    Person = person, DepartmentId = dept.Id, EmploymentType = type,
                    Position = position, TeachingLoad = load, StartDate = DateTime.UtcNow
                });
                changed = true;
            }
            else if (employment.EmploymentType != type || employment.Position != position || employment.TeachingLoad != load)
            {
                employment.EmploymentType = type; employment.Position = position; employment.TeachingLoad = load;
                changed = true;
            }

            if (createdPerson) sr.Created++;
            else if (changed) sr.Updated++;
            else sr.Unchanged++;
            await context.SaveChangesAsync();
        }
    }

    // ===================== Discipline =====================
    private async Task ImportSubjectsAsync(IXLWorksheet ws, ImportReport report)
    {
        var sr = new SheetReport { Sheet = "Discipline" };
        report.Sheets.Add(sr);
        var cols = Columns(ws);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in DataRows(ws, cols))
        {
            int r = row.RowNumber();
            var code = Text(row, cols, "Cod");
            var name = Text(row, cols, "Denumire");
            if (code is null) { sr.Errors.Add(new(r, "Codul este obligatoriu.")); continue; }
            if (name is null) { sr.Errors.Add(new(r, "Denumirea este obligatorie.")); continue; }

            int credits = 0, hours = 0;
            var cc = row.Cell(cols["Credite ECTS"]); var hc = row.Cell(cols["Total ore"]);
            if (!cc.IsEmpty() && (!cc.TryGetValue(out credits) || credits < 0))
            { sr.Errors.Add(new(r, "Creditele ECTS trebuie să fie un număr întreg ≥ 0.")); continue; }
            if (!hc.IsEmpty() && (!hc.TryGetValue(out hours) || hours < 0))
            { sr.Errors.Add(new(r, "Totalul de ore trebuie să fie un număr întreg ≥ 0.")); continue; }

            if (!seen.Add(code + "|" + name))
            { sr.Errors.Add(new(r, $"Disciplina „{code} {name}” apare de mai multe ori în foaie.")); continue; }

            var existing = await context.Subjects.FirstOrDefaultAsync(s => s.Code.ToLower() == code.ToLower() && s.Name.ToLower() == name.ToLower());
            if (existing is null)
            {
                context.Subjects.Add(new Subject { Code = code, Name = name, Credits = credits, TotalHours = hours });
                sr.Created++;
            }
            else if (existing.Credits != credits || existing.TotalHours != hours)
            {
                existing.Credits = credits; existing.TotalHours = hours;
                sr.Updated++;
            }
            else sr.Unchanged++;
            await context.SaveChangesAsync();
        }
    }

    // ===================== utilitare =====================
    private static bool Set(Func<string> get, Action<string> set, string value)
    {
        if (get() == value) return false;
        set(value);
        return true;
    }

    /// <summary>Antetul din rândul 2 (fără " *") → numărul coloanei.</summary>
    private static Dictionary<string, int> Columns(IXLWorksheet ws)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var last = ws.Row(HeaderRow).LastCellUsed()?.Address.ColumnNumber ?? 0;
        for (int c = 1; c <= last; c++)
        {
            var h = ws.Cell(HeaderRow, c).GetString().Trim();
            if (h.EndsWith('*')) h = h[..^1].Trim();
            if (h != "") map[h] = c;
        }
        return map;
    }

    private static IEnumerable<IXLRow> DataRows(IXLWorksheet ws, Dictionary<string, int> cols)
    {
        var last = ws.LastRowUsed()?.RowNumber() ?? 0;
        for (int r = FirstDataRow; r <= last; r++)
        {
            var row = ws.Row(r);
            if (cols.Values.All(c => row.Cell(c).IsEmpty())) continue;
            yield return row;
        }
    }

    private static string? Text(IXLRow row, Dictionary<string, int> cols, string header)
    {
        if (!cols.TryGetValue(header, out var c)) return null;
        var s = row.Cell(c).GetString().Trim();
        return s == "" ? null : s;
    }
}
