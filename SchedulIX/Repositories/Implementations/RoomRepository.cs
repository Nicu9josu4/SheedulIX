using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using SchedulIX.Data;
using SchedulIX.Models.Entities;
using SchedulIX.Repositories.Interfaces;

namespace SchedulIX.Repositories.Implementations
{
    public class RoomRepository(ScheduleDbContext context) : IRoomRepository
    {
        public async Task<Room?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await context.Rooms
                .Include(r => r.RoomType)
                .Include(r => r.Availabilities)
                .FirstOrDefaultAsync(r => r.Id == id, ct);
        }

        public async Task<List<Room>> GetAllAsync(CancellationToken ct = default)
        {
            var result = await context.Rooms
                .AsNoTracking()
                .Include(r => r.RoomType)
                .Include(r => r.Availabilities)
                .ToListAsync(ct);
            return result;
        }

        public async Task<Room> CreateAsync(Room room, CancellationToken ct = default)
        {
            context.Rooms.Add(room);
            await context.SaveChangesAsync(ct);

            // Re-query with navigation properties loaded
            return await context.Rooms
                .Include(r => r.RoomType)
                .Include(r => r.Availabilities)
                .FirstAsync(r => r.Id == room.Id, ct);
        }

        public async Task<Room> UpdateAsync(Room room, CancellationToken ct = default)
        {
            context.Rooms.Update(room);
            await context.SaveChangesAsync(ct);
            return room;
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var room = await context.Rooms.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (room != null)
            {
                context.Rooms.Remove(room);
                await context.SaveChangesAsync(ct);
            }
        }

        public async Task<List<Room>> GetByTypeAsync(int roomTypeId, CancellationToken ct = default)
        {
            return await context.Rooms
                .Include(r => r.RoomType)
                .Include(r => r.Availabilities)
                .Where(r => r.RoomTypeId == roomTypeId)
                .ToListAsync(ct);
        }
    }
}
