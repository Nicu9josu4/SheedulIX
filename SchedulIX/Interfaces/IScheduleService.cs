using SchedulIX.Models;
using SchedulIX.Controllers;

namespace SchedulIX.Interfaces
{
    public interface IScheduleService
    {
        Task<ScheduleDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<ScheduleDto> GenerateScheduleAsync(GenerateScheduleRequestDto request, CancellationToken ct = default);
        Task<ValidationResultDto> ValidateScheduleAsync(ScheduleDto schedule, CancellationToken ct = default);
    }
}
