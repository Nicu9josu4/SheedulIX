using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using SchedulIX.Repositories.Interfaces;
using SchedulIX.Services.Interfaces;

namespace SchedulIX.Services.Implementations
{
    public class ExportService : IExportService
    {
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IRoomRepository _roomRepository;

        public ExportService(IScheduleRepository scheduleRepository, IRoomRepository roomRepository)
        {
            _scheduleRepository = scheduleRepository;
            _roomRepository = roomRepository;
        }

        public async Task<Stream> ExportToExcelAsync(CancellationToken ct = default)
        {
            var schedules = await _scheduleRepository.GetAllAsync(ct);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Orar");

                // Headers
                worksheet.Cell(1, 1).Value = "Disciplina";
                worksheet.Cell(1, 2).Value = "Profesor";
                worksheet.Cell(1, 3).Value = "Sala";
                worksheet.Cell(1, 4).Value = "Grupa";
                worksheet.Cell(1, 5).Value = "Ziua";
                worksheet.Cell(1, 6).Value = "Ora Început";
                worksheet.Cell(1, 7).Value = "Ora Sfârsit";
                worksheet.Cell(1, 8).Value = "Tip Curs";
                worksheet.Cell(1, 9).Value = "Tip Săptămână";

                // Format header row
                var headerRange = worksheet.Range("A1:I1");
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Font.Bold = true;

                // Data rows
                int row = 2;
                foreach (var schedule in schedules.OrderBy(s => s.DayOfWeek).ThenBy(s => s.TimeSlotNumber))
                {
                    worksheet.Cell(row, 1).Value = schedule.Discipline.Name;
                    worksheet.Cell(row, 2).Value = $"{schedule.Teacher.FirstName} {schedule.Teacher.LastName}";
                    worksheet.Cell(row, 3).Value = schedule.Room.RoomNumber;
                    worksheet.Cell(row, 4).Value = schedule.Group?.Name ?? "N/A";
                    worksheet.Cell(row, 5).Value = GetDayName(schedule.DayOfWeek);
                    worksheet.Cell(row, 6).Value = schedule.TimeSlot.StartTime.ToString(@"hh\:mm");
                    worksheet.Cell(row, 7).Value = schedule.TimeSlot.EndTime.ToString(@"hh\:mm");
                    worksheet.Cell(row, 8).Value = schedule.ClassType;
                    worksheet.Cell(row, 9).Value = GetWeekTypeName(schedule.WeekType);

                    row++;
                }

                // Adjust column widths
                worksheet.Columns().AdjustToContents();

                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;
                return stream;
            }
        }

        public async Task<Stream> ExportToPdfAsync(CancellationToken ct = default)
        {
            var schedules = await _scheduleRepository.GetAllAsync(ct);

            var stream = new MemoryStream();

            // Create PDF document
            Document doc = new Document(PageSize.A4.Rotate(), 10f, 10f, 10f, 10f);
            PdfWriter writer = PdfWriter.GetInstance(doc, stream);

            doc.Open();

            // Add title
            Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            Paragraph title = new Paragraph("Program Academic - Orar Curs", titleFont)
            {
                Alignment = Element.ALIGN_CENTER
            };
            doc.Add(title);

            doc.Add(new Paragraph(" "));

            // Create table
            PdfPTable table = new PdfPTable(9);
            table.WidthPercentage = 100;

            // Header cells
            string[] headers = { "Disciplina", "Profesor", "Sala", "Grupa", "Ziua", "Început", "Sfârsit", "Tip", "Săptămână" };
            foreach (var header in headers)
            {
                PdfPCell cell = new PdfPCell(new Phrase(header))
                {
                    BackgroundColor = new BaseColor(200, 200, 200)
                };
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
            }

            // Data rows
            foreach (var schedule in schedules.OrderBy(s => s.DayOfWeek).ThenBy(s => s.TimeSlotNumber))
            {
                table.AddCell(schedule.Discipline.Name);
                table.AddCell($"{schedule.Teacher.FirstName} {schedule.Teacher.LastName}");
                table.AddCell(schedule.Room.RoomNumber);
                table.AddCell(schedule.Group?.Name ?? "N/A");
                table.AddCell(GetDayName(schedule.DayOfWeek));
                table.AddCell(schedule.TimeSlot.StartTime.ToString(@"hh\:mm"));
                table.AddCell(schedule.TimeSlot.EndTime.ToString(@"hh\:mm"));
                table.AddCell(schedule.ClassType);
                table.AddCell(GetWeekTypeName(schedule.WeekType));
            }

            doc.Add(table);

            // Add footer
            doc.Add(new Paragraph(" "));
            Font footerFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            Paragraph footer = new Paragraph($"Generat: {DateTime.Now:dd-MM-yyyy HH:mm}", footerFont)
            {
                Alignment = Element.ALIGN_CENTER
            };
            doc.Add(footer);

            doc.Close();
            writer.Close();

            stream.Position = 0;
            return stream;
        }

        public async Task<Stream> ExportRoomScheduleToExcelAsync(int roomId, CancellationToken ct = default)
        {
            var room = await _roomRepository.GetByIdAsync(roomId, ct);
            if (room is null)
                throw new ArgumentException($"Sala cu ID-ul {roomId} nu a fost găsită.");

            var schedules = await _scheduleRepository.GetByRoomAsync(roomId, ct);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add($"Sala {room.RoomNumber}");

                // Headers
                worksheet.Cell(1, 1).Value = $"Calendarul Sălii: {room.RoomNumber}";
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 14;

                worksheet.Cell(3, 1).Value = "Disciplina";
                worksheet.Cell(3, 2).Value = "Profesor";
                worksheet.Cell(3, 3).Value = "Grupa";
                worksheet.Cell(3, 4).Value = "Ziua";
                worksheet.Cell(3, 5).Value = "Ora Început";
                worksheet.Cell(3, 6).Value = "Ora Sfârsit";
                worksheet.Cell(3, 7).Value = "Tip Curs";

                var headerRange = worksheet.Range("A3:G3");
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange.Style.Font.Bold = true;

                // Data rows
                int row = 4;
                foreach (var schedule in schedules)
                {
                    worksheet.Cell(row, 1).Value = schedule.Discipline.Name;
                    worksheet.Cell(row, 2).Value = $"{schedule.Teacher.FirstName} {schedule.Teacher.LastName}";
                    worksheet.Cell(row, 3).Value = schedule.Group?.Name ?? "N/A";
                    worksheet.Cell(row, 4).Value = GetDayName(schedule.DayOfWeek);
                    worksheet.Cell(row, 5).Value = schedule.TimeSlot.StartTime.ToString(@"hh\:mm");
                    worksheet.Cell(row, 6).Value = schedule.TimeSlot.EndTime.ToString(@"hh\:mm");
                    worksheet.Cell(row, 7).Value = schedule.ClassType;

                    row++;
                }

                worksheet.Columns().AdjustToContents();

                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;
                return stream;
            }
        }

        private static string GetDayName(int dayOfWeek)
        {
            return dayOfWeek switch
            {
                1 => "Luni",
                2 => "Marți",
                3 => "Miercuri",
                4 => "Joi",
                5 => "Vineri",
                6 => "Sâmbătă",
                _ => "Necunoscut"
            };
        }

        private static string GetWeekTypeName(int weekType)
        {
            return weekType switch
            {
                0 => "Toate",
                1 => "Pară",
                2 => "Impară",
                _ => "Necunoscut"
            };
        }
    }
}
