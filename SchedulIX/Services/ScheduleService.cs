using SchedulIX.Interfaces;
using SchedulIX.Models;

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
            // 1. Încărcare resurse (Profesori, Săli, Grupe) din DB
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

        private static List<ScheduleItemDto> GetMockItems() => new()
        {
            new("Programare C#", "Prof. Popescu", "CR-211", "Sala 301", DayOfWeek.Monday, new TimeSpan(8, 0, 0), new TimeSpan(9, 30, 0)),
            new("Baze de Date", "Conf. Ionescu", "CR-211", "Lab 105", DayOfWeek.Monday, new TimeSpan(9, 45, 0), new TimeSpan(11, 15, 0)),
            new("Arhitectură Software", "Prof. Popescu", "CR-211", "Sala 202", DayOfWeek.Tuesday, new TimeSpan(11, 30, 0), new TimeSpan(13, 0, 0))
        };
    }
}
