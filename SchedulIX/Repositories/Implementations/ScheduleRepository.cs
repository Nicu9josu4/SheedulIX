using Microsoft.EntityFrameworkCore;
using SchedulIX.Data;
using SchedulIX.Models.Entities;
using SchedulIX.Repositories.Interfaces;

namespace SchedulIX.Repositories.Implementations
{
    public class ScheduleRepository(ScheduleDbContext context) : IScheduleRepository
    {
        public async Task<Schedule?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await context.Schedules
                .Include(s => s.Discipline)
                .Include(s => s.Teacher)
                .Include(s => s.Room)
                .Include(s => s.Group)
                .Include(s => s.Subgroup)
                .Include(s => s.TimeSlot)
                .FirstOrDefaultAsync(s => s.Id == id, ct);
        }

        public async Task<List<Schedule>> GetAllAsync(CancellationToken ct = default)
        {
            return await context.Schedules
                .Include(s => s.Discipline)
                .Include(s => s.Teacher)
                .Include(s => s.Room)
                .Include(s => s.Group)
                .Include(s => s.Subgroup)
                .Include(s => s.TimeSlot)
                .ToListAsync(ct);
        }

        public async Task<Schedule> CreateAsync(Schedule schedule, CancellationToken ct = default)
        {
            context.Schedules.Add(schedule);
            await context.SaveChangesAsync(ct);
            return schedule;
        }

        public async Task<List<Schedule>> CreateBatchAsync(List<Schedule> schedules, CancellationToken ct = default)
        {
            context.Schedules.AddRange(schedules);
            await context.SaveChangesAsync(ct);
            return schedules;
        }

        public async Task<Schedule> UpdateAsync(Schedule schedule, CancellationToken ct = default)
        {
            context.Schedules.Update(schedule);
            await context.SaveChangesAsync(ct);
            return schedule;
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var schedule = await context.Schedules.FirstOrDefaultAsync(s => s.Id == id, ct);
            if (schedule != null)
            {
                context.Schedules.Remove(schedule);
                await context.SaveChangesAsync(ct);
            }
        }

        public async Task<List<Schedule>> GetByRoomAsync(int roomId, CancellationToken ct = default)
        {
            return await context.Schedules
                .Include(s => s.Discipline)
                .Include(s => s.Teacher)
                .Include(s => s.Group)
                .Include(s => s.Subgroup)
                .Include(s => s.TimeSlot)
                .Where(s => s.RoomId == roomId)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.TimeSlotNumber)
                .ToListAsync(ct);
        }

        public async Task<List<Schedule>> GetByGroupAsync(int groupId, CancellationToken ct = default)
        {
            return await context.Schedules
                .Include(s => s.Discipline)
                .Include(s => s.Teacher)
                .Include(s => s.Room)
                .Include(s => s.TimeSlot)
                .Where(s => s.GroupId == groupId)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.TimeSlotNumber)
                .ToListAsync(ct);
        }

        public async Task<List<Schedule>> GetByTeacherAsync(int teacherId, CancellationToken ct = default)
        {
            return await context.Schedules
                .Include(s => s.Discipline)
                .Include(s => s.Room)
                .Include(s => s.Group)
                .Include(s => s.TimeSlot)
                .Where(s => s.TeacherId == teacherId)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.TimeSlotNumber)
                .ToListAsync(ct);
        }
        public async Task<List<AcademicGroup>> GetGroupsByIdsAsync(List<string> groupIds, CancellationToken ct = default)
        {
            var result = await context.AcademicGroups
                .Include(g => g.Subgroups)
                .Where(g => groupIds.Contains(g.Name))
                .ToListAsync(ct);

            return result;
        }

        public async Task<List<Room>> GetAvailableRoomsWithDetailsAsync(CancellationToken ct = default)
        {
            return await context.Rooms
                .Include(r => r.RoomType)
                .Include(r => r.Availabilities)
                .Where(r => r.IsAvailable)
                .ToListAsync(ct);
        }

        public async Task<List<Teacher>> GetTeachersWithPreferencesAsync(CancellationToken ct = default)
        {
            return await context.Teachers
                .Include(t => t.Preferences)
                .ToListAsync(ct);
        }

        public async Task<List<TimeSlot>> GetOrderedTimeSlotsAsync(CancellationToken ct = default)
        {
            return await context.TimeSlots
                .OrderBy(ts => ts.SlotNumber)
                .ToListAsync(ct);
        }
        public async Task<List<Schedule>> GetScheduleItemsByGroupIdsAsync(List<string> groupIds, CancellationToken ct = default)
        {
            var numericGroupIds = groupIds
                .Select(id => int.TryParse(id, out var parsed) ? parsed : (int?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList();

            return await context.Schedules
                .Include(s => s.Discipline)
                .Include(s => s.Teacher)
                .Include(s => s.Group)
                .Include(s => s.Room)
                .Include(s => s.TimeSlot)
                .Where(s => s.GroupId.HasValue && numericGroupIds.Contains(s.GroupId.Value))
                .ToListAsync(ct);
        }
    }
}
