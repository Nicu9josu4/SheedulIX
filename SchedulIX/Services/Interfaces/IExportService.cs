namespace SchedulIX.Services.Interfaces
{
    public interface IExportService
    {
        Task<Stream> ExportToExcelAsync(CancellationToken ct = default);
        Task<Stream> ExportToPdfAsync(CancellationToken ct = default);
        Task<Stream> ExportRoomScheduleToExcelAsync(int roomId, CancellationToken ct = default);
    }
}
