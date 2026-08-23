using SchedulIX.Models.Entities;

namespace SchedulIX.Repositories.Interfaces
{
    public interface IScheduleRepository
    {
        Task<Schedule?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<Schedule>> GetAllAsync(CancellationToken ct = default);
        Task<Schedule> CreateAsync(Schedule schedule, CancellationToken ct = default);
        Task<List<Schedule>> CreateBatchAsync(List<Schedule> schedules, CancellationToken ct = default);
        Task<Schedule> UpdateAsync(Schedule schedule, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
        Task<List<Schedule>> GetByRoomAsync(int roomId, CancellationToken ct = default);
        Task<List<Schedule>> GetByGroupAsync(int groupId, CancellationToken ct = default);
        Task<List<Schedule>> GetByTeacherAsync(int teacherId, CancellationToken ct = default);
    }
}
