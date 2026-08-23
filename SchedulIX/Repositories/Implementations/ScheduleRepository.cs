using Microsoft.EntityFrameworkCore;
using SchedulIX.Data;
using SchedulIX.Models.Entities;
using SchedulIX.Repositories.Interfaces;

namespace SchedulIX.Repositories.Implementations
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly ScheduleDbContext _context;

        public ScheduleRepository(ScheduleDbContext context)
        {
            _context = context;
        }

        public async Task<Schedule?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.Schedules
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
            return await _context.Schedules
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
            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync(ct);
            return schedule;
        }

        public async Task<List<Schedule>> CreateBatchAsync(List<Schedule> schedules, CancellationToken ct = default)
        {
            _context.Schedules.AddRange(schedules);
            await _context.SaveChangesAsync(ct);
            return schedules;
        }

        public async Task<Schedule> UpdateAsync(Schedule schedule, CancellationToken ct = default)
        {
            _context.Schedules.Update(schedule);
            await _context.SaveChangesAsync(ct);
            return schedule;
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var schedule = await _context.Schedules.FirstOrDefaultAsync(s => s.Id == id, ct);
            if (schedule != null)
            {
                _context.Schedules.Remove(schedule);
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task<List<Schedule>> GetByRoomAsync(int roomId, CancellationToken ct = default)
        {
            return await _context.Schedules
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
            return await _context.Schedules
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
            return await _context.Schedules
                .Include(s => s.Discipline)
                .Include(s => s.Room)
                .Include(s => s.Group)
                .Include(s => s.TimeSlot)
                .Where(s => s.TeacherId == teacherId)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.TimeSlotNumber)
                .ToListAsync(ct);
        }
    }
}
