using Microsoft.EntityFrameworkCore;
using SchedulIX.Data;
using SchedulIX.Models.Entities;
using SchedulIX.Repositories.Interfaces;

namespace SchedulIX.Repositories.Implementations
{
    public class RoomRepository : IRoomRepository
    {
        private readonly ScheduleDbContext _context;

        public RoomRepository(ScheduleDbContext context)
        {
            _context = context;
        }

        public async Task<Room?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.Rooms
                .Include(r => r.RoomType)
                .Include(r => r.Availabilities)
                .FirstOrDefaultAsync(r => r.Id == id, ct);
        }

        public async Task<List<Room>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Rooms
                .Include(r => r.RoomType)
                .Include(r => r.Availabilities)
                .ToListAsync(ct);
        }

        public async Task<Room> CreateAsync(Room room, CancellationToken ct = default)
        {
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync(ct);
            return room;
        }

        public async Task<Room> UpdateAsync(Room room, CancellationToken ct = default)
        {
            _context.Rooms.Update(room);
            await _context.SaveChangesAsync(ct);
            return room;
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (room != null)
            {
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task<List<Room>> GetByTypeAsync(int roomTypeId, CancellationToken ct = default)
        {
            return await _context.Rooms
                .Include(r => r.RoomType)
                .Include(r => r.Availabilities)
                .Where(r => r.RoomTypeId == roomTypeId)
                .ToListAsync(ct);
        }
    }
}
