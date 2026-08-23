using SchedulIX.Models.Entities;

namespace SchedulIX.Repositories.Interfaces
{
    public interface IRoomRepository
    {
        Task<Room?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<Room>> GetAllAsync(CancellationToken ct = default);
        Task<Room> CreateAsync(Room room, CancellationToken ct = default);
        Task<Room> UpdateAsync(Room room, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
        Task<List<Room>> GetByTypeAsync(int roomTypeId, CancellationToken ct = default);
    }
}
