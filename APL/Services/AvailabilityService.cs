using AcademicManagement.Data;
using AcademicManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace AcademicManagement.Services;

public record BlockDto(int Day, int Slot, AvailabilityState State);
public record CommitmentDto(int Day, string Start, string End, string Location);   // Start/End: "HH:mm"
public record SlotDto(int Slot, string Start, string End, bool IsLunchBreak);

public record AvailabilityDto(
    string AcademicYear,
    int? MaxTeachingDays,
    string Notes,
    List<BlockDto> Blocks,
    List<CommitmentDto> Commitments);

public record AvailabilityView(
    List<SlotDto> Slots,
    AvailabilityDto Availability,
    List<BlockDto> ExternallyBusy);   // celule acoperite de angajamente externe (doar de afisat)

public record FeasibilityRow(int Semester, int PhysicalHours, decimal PairsPerWeekNeeded, int PairsPerWeekAvailable, int DaysAvailable, bool Ok, string Message);

/// <summary>Disponibilitatea profesorului (grila săptămânală, zile maxime, angajamente externe) și verificarea de fezabilitate.</summary>
public class AvailabilityService(AppDbContext context)
{
    /// <summary>Numărul de săptămâni ale unui semestru, folosit la transformarea orelor semestriale în ore pe săptămână.</summary>
    public const int WeeksPerSemester = 14;
    /// <summary>O pereche = 2 ore academice.</summary>
    public const int HoursPerPair = 2;

    public async Task<List<TimeSlot>> SlotsAsync() => await context.TimeSlots.OrderBy(s => s.SlotNumber).ToListAsync();

    public async Task<AvailabilityView> GetAsync(Guid professorId, string academicYear)
    {
        await EnsureProfessorAsync(professorId, academicYear);
        var slots = await SlotsAsync();
        var blocks = await context.AvailabilityBlocks
            .Where(b => b.ProfessorProfileId == professorId && b.AcademicYear == academicYear)
            .OrderBy(b => b.DayOfWeek).ThenBy(b => b.SlotNumber).ToListAsync();
        var pref = await context.AvailabilityPreferences
            .FirstOrDefaultAsync(p => p.ProfessorProfileId == professorId && p.AcademicYear == academicYear);
        var commitments = await context.ExternalCommitments
            .Where(c => c.ProfessorProfileId == professorId && c.AcademicYear == academicYear)
            .OrderBy(c => c.DayOfWeek).ThenBy(c => c.StartTime).ToListAsync();

        var dto = new AvailabilityDto(academicYear, pref?.MaxTeachingDays, pref?.Notes ?? "",
            blocks.Select(b => new BlockDto(b.DayOfWeek, b.SlotNumber, b.State)).ToList(),
            commitments.Select(c => new CommitmentDto(c.DayOfWeek, Fmt(c.StartTime), Fmt(c.EndTime), c.Location)).ToList());

        return new AvailabilityView(
            slots.Select(s => new SlotDto(s.SlotNumber, Fmt(s.StartTime), Fmt(s.EndTime), s.IsLunchBreak)).ToList(),
            dto, BusySlots(commitments, slots));
    }

    public async Task SaveAsync(Guid professorId, AvailabilityDto dto)
    {
        await EnsureProfessorAsync(professorId, dto.AcademicYear);
        var slots = await SlotsAsync();
        var slotNumbers = slots.Select(s => s.SlotNumber).ToHashSet();

        if (dto.MaxTeachingDays is < 1 or > 6)
            throw new InvalidOperationException("Numărul maxim de zile de predare trebuie să fie între 1 și 6.");
        if ((dto.Notes ?? "").Length > 2000)
            throw new InvalidOperationException("Observațiile pot avea cel mult 2000 de caractere.");

        // ultima marcare a unei celule câștigă
        var cells = new Dictionary<(int, int), AvailabilityState>();
        foreach (var b in dto.Blocks ?? [])
        {
            if (b.Day is < 1 or > 6) throw new InvalidOperationException($"Zi invalidă: {b.Day} (1 = luni ... 6 = sâmbătă).");
            if (!slotNumbers.Contains(b.Slot)) throw new InvalidOperationException($"Pereche inexistentă: {b.Slot}.");
            cells[(b.Day, b.Slot)] = b.State;
        }

        var commitments = new List<ExternalCommitment>();
        foreach (var c in dto.Commitments ?? [])
        {
            if (c.Day is < 1 or > 6) throw new InvalidOperationException($"Zi invalidă la angajament extern: {c.Day}.");
            if (!TimeSpan.TryParse(c.Start, out var s) || !TimeSpan.TryParse(c.End, out var e) || e <= s)
                throw new InvalidOperationException("Angajament extern: ora de sfârșit trebuie să fie după ora de început (format HH:mm).");
            if ((c.Location ?? "").Length > 200) throw new InvalidOperationException("Locația poate avea cel mult 200 de caractere.");
            commitments.Add(new ExternalCommitment
            {
                ProfessorProfileId = professorId, AcademicYear = dto.AcademicYear,
                DayOfWeek = c.Day, StartTime = s, EndTime = e, Location = (c.Location ?? "").Trim()
            });
        }

        await using var tx = await context.Database.BeginTransactionAsync();
        context.AvailabilityBlocks.RemoveRange(context.AvailabilityBlocks
            .Where(b => b.ProfessorProfileId == professorId && b.AcademicYear == dto.AcademicYear));
        context.ExternalCommitments.RemoveRange(context.ExternalCommitments
            .Where(c => c.ProfessorProfileId == professorId && c.AcademicYear == dto.AcademicYear));
        await context.SaveChangesAsync();

        context.AvailabilityBlocks.AddRange(cells.Select(kv => new AvailabilityBlock
        {
            ProfessorProfileId = professorId, AcademicYear = dto.AcademicYear,
            DayOfWeek = kv.Key.Item1, SlotNumber = kv.Key.Item2, State = kv.Value
        }));
        context.ExternalCommitments.AddRange(commitments);

        var pref = await context.AvailabilityPreferences
            .FirstOrDefaultAsync(p => p.ProfessorProfileId == professorId && p.AcademicYear == dto.AcademicYear);
        if (pref is null)
        {
            pref = new AvailabilityPreference { ProfessorProfileId = professorId, AcademicYear = dto.AcademicYear };
            context.AvailabilityPreferences.Add(pref);
        }
        pref.MaxTeachingDays = dto.MaxTeachingDays;
        pref.Notes = dto.Notes ?? "";
        pref.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        await tx.CommitAsync();
    }

    /// <summary>
    /// Încape sarcina repartizată în disponibilitatea declarată? Compară perechile necesare pe săptămână
    /// (ore pe semestru / 14 săptămâni / 2 ore pe pereche) cu perechile libere, limitate la cele mai bune zile.
    /// </summary>
    public async Task<List<FeasibilityRow>> FeasibilityAsync(Guid professorId, string academicYear)
    {
        var view = await GetAsync(professorId, academicYear);
        var slots = (await SlotsAsync()).Where(s => !s.IsLunchBreak).Select(s => s.SlotNumber).ToList();
        var blocked = view.Availability.Blocks.Where(b => b.State == AvailabilityState.Blocked)
            .Select(b => (b.Day, b.Slot)).Concat(view.ExternallyBusy.Select(b => (b.Day, b.Slot))).ToHashSet();

        var freePerDay = Enumerable.Range(1, 6)
            .Select(d => slots.Count(s => !blocked.Contains((d, s))))
            .OrderByDescending(n => n).ToList();
        int days = view.Availability.MaxTeachingDays ?? 6;
        var usable = freePerDay.Take(days).Where(n => n > 0).ToList();
        int capacity = usable.Sum();

        var hours = await context.TeachingAssignments
            .Where(a => a.ProfessorProfileId == professorId && a.AcademicYear == academicYear
                     && a.Status != AssignmentStatus.Cancelled && a.Status != AssignmentStatus.Archived)
            .GroupBy(a => a.Semester).Select(g => new { Semester = g.Key, Hours = g.Sum(x => x.Hours) }).ToListAsync();

        var rows = new List<FeasibilityRow>();
        foreach (var semester in new[] { 1, 2 })
        {
            int h = hours.FirstOrDefault(x => x.Semester == semester)?.Hours ?? 0;
            decimal needed = Math.Round((decimal)h / WeeksPerSemester / HoursPerPair, 1);
            bool ok = needed <= capacity;
            string msg = h == 0 ? "Nicio repartizare în acest semestru."
                : ok ? $"Încape: {needed} perechi pe săptămână necesare, {capacity} libere în {usable.Count} zile."
                : $"Nu încape: sunt necesare {needed} perechi pe săptămână, dar disponibilitatea declarată oferă doar {capacity} în {usable.Count} zile.";
            rows.Add(new FeasibilityRow(semester, h, needed, capacity, usable.Count, ok, msg));
        }
        return rows;
    }

    private async Task EnsureProfessorAsync(Guid professorId, string academicYear)
    {
        WorkloadService.AcademicYearRange(academicYear);
        if (!await context.ProfessorProfiles.AnyAsync(p => p.Id == professorId))
            throw new KeyNotFoundException("Profesorul nu a fost găsit.");
    }

    private static List<BlockDto> BusySlots(List<ExternalCommitment> commitments, List<TimeSlot> slots) =>
        commitments.SelectMany(c => slots
            .Where(s => !s.IsLunchBreak && c.StartTime < s.EndTime && c.EndTime > s.StartTime)
            .Select(s => new BlockDto(c.DayOfWeek, s.SlotNumber, AvailabilityState.Blocked)))
            .Distinct().ToList();

    private static string Fmt(TimeSpan t) => t.ToString(@"hh\:mm");
}
