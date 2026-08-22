using SchedulIX.Models;

namespace SchedulIX.Interfaces
{
    public interface IScheduleService
    {
        Task<ScheduleDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<ScheduleDto> GenerateScheduleAsync(GenerateScheduleRequestDto request, CancellationToken ct = default);
    }
}
