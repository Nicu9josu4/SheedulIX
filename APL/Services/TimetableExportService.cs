using AcademicManagement.Data;
using AcademicManagement.Domain;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AcademicManagement.Services;

public enum ExportView { Group, Professor, Room, All }

/// <summary>Export al orarului curent: Excel (grupe, profesori, săli, neplasate) și PDF gata de tipar (câte o pagină pe grupă / profesor / sală).</summary>
public class TimetableExportService(AppDbContext context, TimetableService timetable)
{
    private static readonly string[] DayNames = ["Luni", "Marți", "Miercuri", "Joi", "Vineri", "Sâmbătă"];

    static TimetableExportService()
    {
        // QuestPDF Community License: verifică dacă se aplică instituției tale (https://www.questpdf.com/license/).
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private sealed record Block(string Title, List<TimetableEntryView> Entries);

    private async Task<(List<SlotDto> Slots, List<TimetableEntryView> Entries)> LoadAsync(string academicYear, int semester)
    {
        WorkloadService.AcademicYearRange(academicYear);
        if (semester is < 1 or > 2) throw new InvalidOperationException("Semestrul trebuie să fie 1 sau 2.");
        var entries = await timetable.EntriesAsync(academicYear, semester, null, null, null);
        if (entries.Count == 0)
            throw new InvalidOperationException("Nu există un orar generat pentru acest an și semestru.");
        var slots = (await context.TimeSlots.Where(s => !s.IsLunchBreak).OrderBy(s => s.SlotNumber).ToListAsync())
            .Select(s => new SlotDto(s.SlotNumber, s.StartTime.ToString(@"hh\:mm"), s.EndTime.ToString(@"hh\:mm"), false)).ToList();
        return (slots, entries);
    }

    private static List<Block> Blocks(List<TimetableEntryView> entries, ExportView view, Guid? id)
    {
        // id (opțional) restrânge la o singură grupă / un singur profesor / o singură sală, când vizualizarea este una singură
        bool only = id is not null && view != ExportView.All;
        var all = new List<Block>();
        if (view is ExportView.Group or ExportView.All)
            all.AddRange(entries.GroupBy(e => (e.GroupId, e.Group)).OrderBy(g => g.Key.Group)
                .Where(g => !only || g.Key.GroupId == id)
                .Select(g => new Block("Grupa " + g.Key.Group, g.ToList())));
        if (view is ExportView.Professor or ExportView.All)
            all.AddRange(entries.GroupBy(e => (e.ProfessorId, e.Professor)).OrderBy(g => g.Key.Professor)
                .Where(g => !only || g.Key.ProfessorId == id)
                .Select(g => new Block("Profesor: " + g.Key.Professor, g.ToList())));
        if (view is ExportView.Room or ExportView.All)
            all.AddRange(entries.GroupBy(e => (e.RoomId, e.Room, e.Building)).OrderBy(g => g.Key.Building).ThenBy(g => g.Key.Room)
                .Where(g => !only || g.Key.RoomId == id)
                .Select(g => new Block($"Sala {g.Key.Room} ({g.Key.Building})", g.ToList())));
        return all;
    }

    // ============================== Excel ==============================
    public async Task<byte[]> ExcelAsync(string academicYear, int semester)
    {
        var (slots, entries) = await LoadAsync(academicYear, semester);
        var summary = await timetable.LatestSummaryAsync(academicYear, semester);

        using var wb = new XLWorkbook();
        var info = wb.AddWorksheet("Rezumat");
        info.Cell(1, 1).Value = "Orar"; info.Cell(1, 1).Style.Font.Bold = true; info.Cell(1, 1).Style.Font.FontSize = 16;
        info.Cell(2, 1).Value = "An universitar"; info.Cell(2, 2).Value = academicYear;
        info.Cell(3, 1).Value = "Semestru"; info.Cell(3, 2).Value = semester;
        if (summary is not null)
        {
            info.Cell(4, 1).Value = "Perechi plasate"; info.Cell(4, 2).Value = $"{summary.SessionsPlaced} / {summary.SessionsTotal}";
            info.Cell(5, 1).Value = "Generat"; info.Cell(5, 2).Value = $"{summary.SolverStatus}, {summary.SolveSeconds} s, penalizare {summary.Penalty}";
        }
        info.Column(1).Width = 18; info.Column(2).Width = 40;

        WriteSheet(wb, "Grupe", Blocks(entries, ExportView.Group, null), slots);
        WriteSheet(wb, "Profesori", Blocks(entries, ExportView.Professor, null), slots);
        WriteSheet(wb, "Săli", Blocks(entries, ExportView.Room, null), slots);

        var un = wb.AddWorksheet("Neplasate");
        string[] heads = ["Profesor", "Disciplină", "Grupă", "Activitate", "Perechi", "Motiv"];
        for (int c = 0; c < heads.Length; c++) { un.Cell(1, c + 1).Value = heads[c]; un.Cell(1, c + 1).Style.Font.Bold = true; }
        int r = 2;
        foreach (var u in summary?.Unplaced ?? [])
        {
            un.Cell(r, 1).Value = u.Professor; un.Cell(r, 2).Value = u.Subject; un.Cell(r, 3).Value = u.Group;
            un.Cell(r, 4).Value = ActivityRo(u.Activity); un.Cell(r, 5).Value = u.Sessions; un.Cell(r, 6).Value = u.Reason; r++;
        }
        if (r == 2) un.Cell(2, 1).Value = "Toate perechile au fost plasate.";
        un.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private static void WriteSheet(XLWorkbook wb, string name, List<Block> blocks, List<SlotDto> slots)
    {
        var ws = wb.AddWorksheet(name);
        ws.Column(1).Width = 12;
        for (int c = 0; c < slots.Count; c++) ws.Column(c + 2).Width = 30;
        int row = 1;
        foreach (var b in blocks)
        {
            var title = ws.Range(row, 1, row, slots.Count + 1).Merge();
            title.Value = b.Title; title.Style.Font.Bold = true; title.Style.Font.FontSize = 13;
            row++;

            ws.Cell(row, 1).Value = "Ziua";
            for (int c = 0; c < slots.Count; c++) ws.Cell(row, c + 2).Value = $"Pereche {slots[c].Slot}\n{slots[c].Start}–{slots[c].End}";
            var head = ws.Range(row, 1, row, slots.Count + 1);
            head.Style.Font.Bold = true; head.Style.Font.FontColor = XLColor.White; head.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F4E79");
            head.Style.Alignment.WrapText = true; head.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row++;

            for (int d = 1; d <= 6; d++)
            {
                ws.Cell(row, 1).Value = DayNames[d - 1]; ws.Cell(row, 1).Style.Font.Bold = true;
                for (int c = 0; c < slots.Count; c++)
                {
                    var cell = b.Entries.Where(e => e.Day == d && e.Slot == slots[c].Slot).ToList();
                    var xl = ws.Cell(row, c + 2);
                    xl.Value = string.Join("\n\n", cell.Select(e => string.Join("\n", Lines(e))));
                    if (cell.Count > 0) xl.Style.Fill.BackgroundColor = XLColor.FromHtml(ColorHex(cell[0].Activity));
                }
                var line = ws.Range(row, 1, row, slots.Count + 1);
                line.Style.Alignment.WrapText = true; line.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                ws.Row(row).AdjustToContents();
                row++;
            }
            var grid = ws.Range(row - 8, 1, row - 1, slots.Count + 1);
            grid.Style.Border.InsideBorder = XLBorderStyleValues.Thin; grid.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            row++;
        }
    }

    // ============================== PDF ==============================
    public async Task<byte[]> PdfAsync(string academicYear, int semester, ExportView view, Guid? id)
    {
        var (slots, entries) = await LoadAsync(academicYear, semester);
        var blocks = Blocks(entries, view, id);
        if (blocks.Count == 0) throw new InvalidOperationException("Nu există intrări de orar pentru selecția cerută.");

        return Document.Create(doc =>
        {
            foreach (var b in blocks)
                doc.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(22);
                    page.DefaultTextStyle(t => t.FontSize(8));

                    page.Header().Column(c =>
                    {
                        c.Item().Text(b.Title).FontSize(15).Bold();
                        c.Item().Text($"An universitar {academicYear}, semestrul {semester}").FontColor(Colors.Grey.Darken1);
                    });

                    page.Content().PaddingTop(8).Table(t =>
                    {
                        t.ColumnsDefinition(c => { c.ConstantColumn(52); foreach (var _ in slots) c.RelativeColumn(); });
                        t.Header(h =>
                        {
                            h.Cell().Element(Head).Text("Ziua").Bold();
                            foreach (var s in slots)
                                h.Cell().Element(Head).AlignCenter().Text($"Pereche {s.Slot}\n{s.Start}–{s.End}").Bold();
                        });
                        for (int d = 1; d <= 6; d++)
                        {
                            t.Cell().Element(BodyCell).Text(DayNames[d - 1]).Bold();
                            foreach (var s in slots)
                            {
                                var cell = b.Entries.Where(e => e.Day == d && e.Slot == s.Slot).ToList();
                                var color = cell.Count > 0 ? ColorHex(cell[0].Activity) : "#FFFFFF";
                                t.Cell().Element(x => BodyCell(x).Background(color)).Column(col =>
                                {
                                    foreach (var e in cell)
                                        col.Item().PaddingBottom(3).Column(inner =>
                                        {
                                            var l = Lines(e);
                                            inner.Item().Text(l[0]).Bold();
                                            for (int i = 1; i < l.Length; i++) inner.Item().Text(l[i]);
                                        });
                                });
                            }
                        }
                    });

                    page.Footer().Row(r =>
                    {
                        r.RelativeItem().Text("Generat cu SchedulIX").FontColor(Colors.Grey.Medium);
                        r.AutoItem().Text(x => { x.Span("Pagina "); x.CurrentPageNumber(); x.Span(" / "); x.TotalPages(); });
                    });
                });
        }).GeneratePdf();

        static IContainer Head(IContainer c) => c.Background("#1F4E79").Padding(4).DefaultTextStyle(x => x.FontColor(Colors.White));
        static IContainer BodyCell(IContainer c) => c.Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(3).MinHeight(62);
    }

    // ============================== comun ==============================
    private static string[] Lines(TimetableEntryView e)
    {
        string who = e.Group + (e.Subgroup is null ? "" : " · " + e.Subgroup.Replace(e.Group + "-", ""));
        string parity = e.Parity switch { "Odd" => " · săpt. impare", "Even" => " · săpt. pare", _ => "" };
        return [e.Subject, $"{ActivityRo(e.Activity)} · {who}", e.Professor, $"{e.Room} ({e.Building}){parity}"];
    }

    private static string ActivityRo(string a) => a switch { "Lecture" => "Curs", "Seminar" => "Seminar", "Laboratory" => "Laborator", _ => a };
    private static string ColorHex(string activity) => activity switch { "Lecture" => "#DCE6FF", "Seminar" => "#D8F3E3", "Laboratory" => "#FFF0C7", _ => "#FFFFFF" };
}
