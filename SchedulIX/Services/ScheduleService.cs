using SchedulIX.Controllers;
using SchedulIX.Interfaces;
using SchedulIX.Models;
using SchedulIX.Models.Entities;
using SchedulIX.Repositories.Interfaces;

namespace SchedulIX.Services
{
    public class ScheduleService(IScheduleRepository scheduleRepository) : IScheduleService
    {
        // Aici se injectează repository-urile și engine-ul de optimizare (ex: OR-Tools sau Algoritm Genetic)
        public async Task<ScheduleDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            // Simulăm citirea unui orar generat din DB
            await Task.Delay(100, ct);

            return new ScheduleDto(
                id,
                "Orar Semestrul 1",
                DateTime.UtcNow,
                GetMockItems(),
                0,
                95
            );
        }

        public async Task<ScheduleDto> GenerateScheduleAsync(GenerateScheduleRequestDto request, CancellationToken ct = default)
        {
            // 1. Validate inputs
            if (request.GroupIds is null || !request.GroupIds.Any())
            {
                throw new ArgumentException("Trebuie să selectați cel puțin o grupă de studenți.", nameof(request));
            }
            var groupIds = request.GroupIds;

            // 2. Fetch required resources from Database
            var targetGroups = await scheduleRepository.GetGroupsByIdsAsync([.. groupIds]);

            if (!targetGroups.Any())
            {
                throw new KeyNotFoundException("Niciuna dintre grupele specificate nu a fost găsită în baza de date.");
            }

            var availableRooms = await scheduleRepository.GetAvailableRoomsWithDetailsAsync();
            var teachers = await scheduleRepository.GetTeachersWithPreferencesAsync();
            var timeSlots = await scheduleRepository.GetOrderedTimeSlotsAsync();

            var items = await scheduleRepository.GetScheduleItemsByGroupIdsAsync([.. groupIds]);
            // 3. Invoke Schedule Solver / Optimization Engine
            // TODO: Pass (targetGroups, availableRooms, teachers, timeSlots, request) to your solver engine
            await Task.Delay(1500, ct); // Simulating algorithm calculation time

            // 4. Construct and return result DTO
            var newScheduleId = Guid.NewGuid();

            return new ScheduleDto(
                Id: newScheduleId,
                Title: $"Orar Generat - {DateTime.Now:yyyy-MM-dd HH:mm}",
                CreatedAt: DateTime.UtcNow,
                Items: MapToScheduleItemDtos(items),
                HardConstraintViolations: 0,
                SoftConstraintScore: 92
            );
        }

        private static List<ScheduleItemDto> MapToScheduleItemDtos(IEnumerable<Schedule> schedules)
        {
            return schedules.Select(s => new ScheduleItemDto(
                SubjectName: s.Discipline?.Name ?? "N/A",
                TeacherName: s.Teacher != null ? $"Prof. {s.Teacher.LastName} {s.Teacher.FirstName}" : "N/A",
                GroupName: s.Group?.Name ?? "N/A",
                RoomName: s.Room?.RoomNumber ?? "N/A",
                Day: (DayOfWeek)s.DayOfWeek,
                StartTime: s.TimeSlot?.StartTime ?? TimeSpan.Zero,
                EndTime: s.TimeSlot?.EndTime ?? TimeSpan.Zero
            )).ToList();
        }

        public async Task<ValidationResultDto> ValidateScheduleAsync(ScheduleDto schedule, CancellationToken ct = default)
        {
            await Task.Delay(100, ct);

            var violations = new List<string>();

            // Validate basic rules
            // 1. No more than 5 periods per day per group
            var itemsByDay = schedule.Items.GroupBy(i => i.Day);
            foreach (var day in itemsByDay)
            {
                if (day.Count() > 5)
                {
                    violations.Add($"Grupa {day.First().GroupName} are mai mult de 5 perechi în ziua {day.Key}");
                }
            }

            return new ValidationResultDto(
                violations.Count == 0,
                violations,
                violations.Count,
                100 - (violations.Count * 5)
            );
        }

        private static List<ScheduleItemDto> GetMockItems() =>
            new()
            {
                new("Programare C#", "Prof. Popescu", "CR-211", "Sala 301", DayOfWeek.Monday, new TimeSpan(8, 0, 0), new TimeSpan(9, 30, 0)),
                new("Baze de Date", "Conf. Ionescu", "CR-211", "Lab 105", DayOfWeek.Monday, new TimeSpan(9, 45, 0), new TimeSpan(11, 15, 0)),
                new("Arhitectură Software", "Prof. Popescu", "CR-211", "Sala 202", DayOfWeek.Tuesday, new TimeSpan(11, 30, 0), new TimeSpan(13, 0, 0))
            };
    }
}
