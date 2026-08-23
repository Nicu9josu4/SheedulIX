using SchedulIX.Interfaces;
using SchedulIX.Models;
using SchedulIX.Controllers;

namespace SchedulIX.Services
{
    public class ScheduleService : IScheduleService
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
            // 1. Încărcarea resurse (Profesori, Săli, Grupe) din DB
            // 2. Apelare Solver / Engine de optimizare
            await Task.Delay(1500, ct); // Simulăm procesul de calcul/generare

            var newScheduleId = Guid.NewGuid();
            return new ScheduleDto(
                newScheduleId,
                $"Orar Generat - {DateTime.Now:yyyy-MM-dd HH:mm}",
                DateTime.UtcNow,
                GetMockItems(),
                HardConstraintViolations: 0,
                SoftConstraintScore: 92
            );
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
