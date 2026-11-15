using AcademicManagement.Data;
using AcademicManagement.Domain;
using Google.OrTools.Sat;
using Microsoft.EntityFrameworkCore;

namespace AcademicManagement.Services;

public record UnplacedDto(Guid TeachingAssignmentId, string Professor, string Subject, string Group, string Activity, int Sessions, string Reason);

public record TimetableSummary(
    Guid RunId, string AcademicYear, int Semester, string SolverStatus,
    int SessionsTotal, int SessionsPlaced, int Penalty, double SolveSeconds, List<UnplacedDto> Unplaced);

public record TimetableEntryView(
    Guid Id, int Day, int Slot, string Start, string End, string Parity,
    Guid AssignmentId, string Subject, string Activity,
    Guid ProfessorId, string Professor, Guid GroupId, string Group, string? Subgroup,
    Guid RoomId, string Room, string Building);

/// <summary>
/// Generarea orarului cu un solver de constrângeri (OR-Tools CP-SAT).
/// Reguli obligatorii: nicio pereche în două locuri deodată (profesor, grupă/subgrupă, sală), perechile profesorului
/// doar în celulele în care nu e blocat sau ocupat extern, sala potrivită ca tip și capacitate, săptămâni pare/impare,
/// perechi consecutive ale unui profesor în clădiri diferite sunt interzise, număr maxim de zile de predare.
/// Reguli moi (minimizate): celule „aș evita”, zile de predare în plus; recompensă pentru „prefer”.
/// Ce nu încape rămâne explicit în lista „neplasate”, cu motiv.
/// </summary>
public class TimetableService(AppDbContext context)
{
    public const int WeeksPerSemester = AvailabilityService.WeeksPerSemester;
    private const int PenaltyUnplaced = 1000, PenaltyAvoid = 3, PenaltyExtraDay = 2, RewardPreferred = -1;
    private const int MaxOptions = 400_000;

    private sealed class Session
    {
        public TeachingAssignment A = null!;
        public bool Weekly;
        public int Students;
        public List<Option> Options = [];
        public BoolVar Unplaced = null!;
        public string? PreReason;
    }

    private sealed record Option(int Day, int Slot, WeekParity Parity, Room Room, BoolVar Var);

    /// <summary>Câte perechi pe săptămână (setul „săptămânal”) și câte „la două săptămâni” rezultă din orele pe semestru.</summary>
    public static (int Weekly, int Biweekly) Pattern(int hours)
    {
        int pairs = (int)Math.Ceiling(hours / 2.0);
        int weekly = pairs / WeeksPerSemester;
        int rest = pairs % WeeksPerSemester;
        int extra = (int)Math.Round(rest / (WeeksPerSemester / 2.0));   // o pereche la 2 săptămâni acoperă ~7,5 perechi
        if (extra >= 2) { weekly++; extra = 0; }
        if (weekly == 0 && extra == 0 && pairs > 0) extra = 1;
        return (weekly, extra);
    }

    public async Task<TimetableSummary> GenerateAsync(string academicYear, int semester, int timeLimitSeconds)
    {
        WorkloadService.AcademicYearRange(academicYear);
        if (semester is < 1 or > 2) throw new InvalidOperationException("Semestrul trebuie să fie 1 sau 2.");
        timeLimitSeconds = Math.Clamp(timeLimitSeconds, 1, 300);

        var slots = (await context.TimeSlots.Where(s => !s.IsLunchBreak).OrderBy(s => s.SlotNumber).ToListAsync());
        var rooms = await context.Rooms.Where(r => r.Active).ToListAsync();
        var assignments = await context.TeachingAssignments
            .Include(a => a.ProfessorProfile).ThenInclude(p => p.Person)
            .Include(a => a.Subject).Include(a => a.Subgroup)
            .Include(a => a.Group).ThenInclude(g => g.Subgroups)
            .Where(a => a.AcademicYear == academicYear && a.Semester == semester
                     && a.Status != AssignmentStatus.Cancelled && a.Status != AssignmentStatus.Archived)
            .ToListAsync();
        if (assignments.Count == 0)
            throw new InvalidOperationException("Nu există repartizări pentru acest an și semestru.");
        if (rooms.Count == 0)
            throw new InvalidOperationException("Nu există nicio sală activă. Adaugă săli înainte de a genera orarul.");

        var profIds = assignments.Select(a => a.ProfessorProfileId).Distinct().ToList();
        var blocks = await context.AvailabilityBlocks
            .Where(b => profIds.Contains(b.ProfessorProfileId) && b.AcademicYear == academicYear).ToListAsync();
        var prefs = await context.AvailabilityPreferences
            .Where(p => profIds.Contains(p.ProfessorProfileId) && p.AcademicYear == academicYear).ToDictionaryAsync(p => p.ProfessorProfileId);
        var commitments = await context.ExternalCommitments
            .Where(c => profIds.Contains(c.ProfessorProfileId) && c.AcademicYear == academicYear).ToListAsync();

        // stare pe celula (profesor, zi, pereche)
        var state = new Dictionary<(Guid, int, int), AvailabilityState>();
        foreach (var b in blocks) state[(b.ProfessorProfileId, b.DayOfWeek, b.SlotNumber)] = b.State;
        foreach (var c in commitments)
            foreach (var s in slots.Where(s => c.StartTime < s.EndTime && c.EndTime > s.StartTime))
                state[(c.ProfessorProfileId, c.DayOfWeek, s.SlotNumber)] = AvailabilityState.Blocked;

        // ===== sesiuni =====
        var sessions = new List<Session>();
        foreach (var a in assignments)
        {
            var (weekly, biweekly) = Pattern(a.Hours);
            int students = a.Subgroup?.StudentCount ?? a.Group.StudentCount;
            for (int i = 0; i < weekly; i++) sessions.Add(new Session { A = a, Weekly = true, Students = students });
            for (int i = 0; i < biweekly; i++) sessions.Add(new Session { A = a, Weekly = false, Students = students });
        }

        var model = new CpModel();
        long optionCount = 0;
        var objTerms = new List<LinearExpr>(); var objCoefs = new List<long>();

        // registru de resurse: (resursa, zi, pereche) -> variabile pe paritate
        var reg = new Dictionary<(string, int, int), (List<ILiteral> All, List<ILiteral> Odd, List<ILiteral> Even)>();
        void Reg(string res, Option o)
        {
            var key = (res, o.Day, o.Slot);
            if (!reg.TryGetValue(key, out var e)) reg[key] = e = ([], [], []);
            (o.Parity switch { WeekParity.All => e.All, WeekParity.Odd => e.Odd, _ => e.Even }).Add(o.Var);
        }

        var dayVars = new Dictionary<(Guid, int), BoolVar>();
        var buildingVars = new Dictionary<(Guid, int, int, string), BoolVar>();
        var profOptions = new Dictionary<Guid, List<Option>>();

        for (int si = 0; si < sessions.Count; si++)
        {
            var s = sessions[si];
            var eligible = EligibleRooms(s.A.ActivityType, s.Students, rooms);
            if (eligible.Count == 0) s.PreReason = "nicio sală activă potrivită ca tip și capacitate";

            var parities = s.Weekly ? new[] { WeekParity.All } : new[] { WeekParity.Odd, WeekParity.Even };
            for (int day = 1; day <= 6 && s.PreReason is null; day++)
                foreach (var slot in slots)
                {
                    var st = state.TryGetValue((s.A.ProfessorProfileId, day, slot.SlotNumber), out var v) ? (AvailabilityState?)v : null;
                    if (st == AvailabilityState.Blocked) continue;
                    foreach (var parity in parities)
                        foreach (var room in eligible)
                        {
                            var x = model.NewBoolVar($"x_{si}_{day}_{slot.SlotNumber}_{parity}_{room.Code}");
                            var o = new Option(day, slot.SlotNumber, parity, room, x);
                            s.Options.Add(o);
                            if (++optionCount > MaxOptions)
                                throw new InvalidOperationException("Problema este prea mare (" + MaxOptions + "+ variante). Generează pe un semestru mai mic sau redu numărul de săli.");

                            if (st == AvailabilityState.Avoid) { objTerms.Add(x); objCoefs.Add(PenaltyAvoid); }
                            else if (st == AvailabilityState.Preferred) { objTerms.Add(x); objCoefs.Add(RewardPreferred); }

                            Reg("P:" + s.A.ProfessorProfileId, o);
                            Reg("R:" + room.Id, o);
                            foreach (var u in UnitResources(s.A)) Reg("U:" + u, o);

                            (profOptions.TryGetValue(s.A.ProfessorProfileId, out var pl) ? pl : profOptions[s.A.ProfessorProfileId] = []).Add(o);
                        }
                }

            if (s.PreReason is null && s.Options.Count == 0)
                s.PreReason = "profesorul nu are celule disponibile (blocaje sau angajamente externe)";

            s.Unplaced = model.NewBoolVar($"u_{si}");
            var lits = s.Options.Select(o => (ILiteral)o.Var).Append(s.Unplaced).ToList();
            model.AddExactlyOne(lits);
            objTerms.Add(s.Unplaced); objCoefs.Add(PenaltyUnplaced);
        }

        // ===== conflicte: o resursă nu poate fi în două locuri; paritățile pare/impare nu se ciocnesc între ele =====
        foreach (var (_, e) in reg)
        {
            if (e.All.Count + e.Odd.Count > 1) model.AddAtMostOne(e.All.Concat(e.Odd));
            if (e.All.Count + e.Even.Count > 1) model.AddAtMostOne(e.All.Concat(e.Even));
        }

        // ===== profesor: zile de predare (soft) + număr maxim (hard) + clădiri diferite în perechi consecutive (hard) =====
        foreach (var (prof, opts) in profOptions)
        {
            foreach (var g in opts.GroupBy(o => o.Day))
            {
                var d = model.NewBoolVar($"day_{prof}_{g.Key}");
                dayVars[(prof, g.Key)] = d;
                foreach (var o in g) model.AddImplication(o.Var, d);
                objTerms.Add(d); objCoefs.Add(PenaltyExtraDay);
            }
            if (prefs.TryGetValue(prof, out var pf) && pf.MaxTeachingDays is { } max)
                model.Add(LinearExpr.Sum(dayVars.Where(kv => kv.Key.Item1 == prof).Select(kv => (LinearExpr)kv.Value)) <= max);

            foreach (var g in opts.GroupBy(o => (o.Day, o.Slot, o.Room.Building)))
            {
                var b = model.NewBoolVar($"b_{prof}_{g.Key.Day}_{g.Key.Slot}_{g.Key.Building}");
                buildingVars[(prof, g.Key.Day, g.Key.Slot, g.Key.Building)] = b;
                foreach (var o in g) model.AddImplication(o.Var, b);
            }
        }
        foreach (var ((prof, day, slot, building), b1) in buildingVars)
            foreach (var other in buildingVars.Where(kv => kv.Key.Item1 == prof && kv.Key.Item2 == day && kv.Key.Item3 == slot + 1 && kv.Key.Item4 != building))
                model.AddBoolOr([b1.Not(), other.Value.Not()]);

        model.Minimize(LinearExpr.WeightedSum(objTerms, objCoefs));

        // ===== rezolvare =====
        var solver = new CpSolver { StringParameters = $"max_time_in_seconds:{timeLimitSeconds}.0 num_workers:8 random_seed:1" };
        var status = solver.Solve(model);
        if (status is not (CpSolverStatus.Optimal or CpSolverStatus.Feasible))
            throw new InvalidOperationException("Solverul nu a găsit nicio soluție (" + status + ").");

        // ===== salvare =====
        await using var tx = await context.Database.BeginTransactionAsync();
        var old = await context.TimetableRuns.Where(r => r.AcademicYear == academicYear && r.Semester == semester).ToListAsync();
        context.TimetableRuns.RemoveRange(old);
        await context.SaveChangesAsync();

        var run = new TimetableRun
        {
            AcademicYear = academicYear, Semester = semester, SolverStatus = status.ToString(),
            SessionsTotal = sessions.Count, TimeLimitSeconds = timeLimitSeconds, SolveSeconds = Math.Round(solver.WallTime(), 2),
            Penalty = (int)solver.ObjectiveValue
        };
        var unplaced = new Dictionary<Guid, TimetableUnplaced>();
        foreach (var s in sessions)
        {
            var chosen = s.Options.FirstOrDefault(o => solver.BooleanValue(o.Var));
            if (chosen is not null)
            {
                run.Entries.Add(new TimetableEntry
                {
                    TeachingAssignmentId = s.A.Id, DayOfWeek = chosen.Day, SlotNumber = chosen.Slot,
                    RoomId = chosen.Room.Id, Parity = chosen.Parity
                });
                continue;
            }
            if (!unplaced.TryGetValue(s.A.Id, out var u))
                unplaced[s.A.Id] = u = new TimetableUnplaced
                {
                    TeachingAssignmentId = s.A.Id,
                    Reason = s.PreReason ?? "nu a rămas loc liber fără conflicte (profesor, grupă sau săli ocupate)"
                };
            u.Sessions++;
        }
        run.SessionsPlaced = run.Entries.Count;
        foreach (var u in unplaced.Values) run.Unplaced.Add(u);
        context.TimetableRuns.Add(run);
        await context.SaveChangesAsync();
        await tx.CommitAsync();

        return await SummaryAsync(run.Id);
    }

    public async Task<TimetableSummary?> LatestSummaryAsync(string academicYear, int semester)
    {
        var id = await context.TimetableRuns.Where(r => r.AcademicYear == academicYear && r.Semester == semester)
            .OrderByDescending(r => r.CreatedAt).Select(r => (Guid?)r.Id).FirstOrDefaultAsync();
        return id is null ? null : await SummaryAsync(id.Value);
    }

    private async Task<TimetableSummary> SummaryAsync(Guid runId)
    {
        var run = await context.TimetableRuns.AsNoTracking().FirstAsync(r => r.Id == runId);
        var unplaced = await context.TimetableUnplaced.AsNoTracking().Where(u => u.TimetableRunId == runId)
            .Select(u => new UnplacedDto(u.TeachingAssignmentId,
                u.TeachingAssignment.ProfessorProfile.Person.LastName + " " + u.TeachingAssignment.ProfessorProfile.Person.FirstName,
                u.TeachingAssignment.Subject.Name, u.TeachingAssignment.Group.Code, u.TeachingAssignment.ActivityType.ToString(),
                u.Sessions, u.Reason)).ToListAsync();
        return new TimetableSummary(run.Id, run.AcademicYear, run.Semester, run.SolverStatus,
            run.SessionsTotal, run.SessionsPlaced, run.Penalty, run.SolveSeconds, unplaced);
    }

    /// <summary>Intrările orarului curent, filtrabile după grupă, profesor sau sală.</summary>
    public async Task<List<TimetableEntryView>> EntriesAsync(string academicYear, int semester, Guid? groupId, Guid? professorId, Guid? roomId)
    {
        var runId = await context.TimetableRuns.Where(r => r.AcademicYear == academicYear && r.Semester == semester)
            .OrderByDescending(r => r.CreatedAt).Select(r => (Guid?)r.Id).FirstOrDefaultAsync();
        if (runId is null) return [];

        var slots = await context.TimeSlots.ToDictionaryAsync(s => s.SlotNumber);
        var q = context.TimetableEntries.AsNoTracking().Where(e => e.TimetableRunId == runId);
        if (groupId is not null) q = q.Where(e => e.TeachingAssignment.GroupId == groupId);
        if (professorId is not null) q = q.Where(e => e.TeachingAssignment.ProfessorProfileId == professorId);
        if (roomId is not null) q = q.Where(e => e.RoomId == roomId);

        var rows = await q.Select(e => new
        {
            e.Id, e.DayOfWeek, e.SlotNumber, e.Parity, e.TeachingAssignmentId,
            Subject = e.TeachingAssignment.Subject.Name, e.TeachingAssignment.ActivityType,
            ProfId = e.TeachingAssignment.ProfessorProfileId,
            Prof = e.TeachingAssignment.ProfessorProfile.Person.LastName + " " + e.TeachingAssignment.ProfessorProfile.Person.FirstName,
            GroupId = e.TeachingAssignment.GroupId, Group = e.TeachingAssignment.Group.Code,
            Subgroup = e.TeachingAssignment.Subgroup != null ? e.TeachingAssignment.Subgroup.Name : null,
            e.RoomId, Room = e.Room.Code, e.Room.Building
        }).ToListAsync();

        return rows.OrderBy(r => r.DayOfWeek).ThenBy(r => r.SlotNumber).Select(r => new TimetableEntryView(
            r.Id, r.DayOfWeek, r.SlotNumber,
            slots[r.SlotNumber].StartTime.ToString(@"hh\:mm"), slots[r.SlotNumber].EndTime.ToString(@"hh\:mm"), r.Parity.ToString(),
            r.TeachingAssignmentId, r.Subject, r.ActivityType.ToString(), r.ProfId, r.Prof, r.GroupId, r.Group, r.Subgroup,
            r.RoomId, r.Room, r.Building)).ToList();
    }

    // ===== reguli ajutătoare =====
    private static List<Room> EligibleRooms(ActivityType type, int students, List<Room> rooms) =>
        rooms.Where(r => r.Capacity >= students && type switch
        {
            ActivityType.Lecture => r.Type == RoomType.Lecture,
            ActivityType.Seminar => r.Type is RoomType.Seminar or RoomType.Lecture,
            _ => r.Type == RoomType.Laboratory
        }).ToList();

    /// <summary>Unitățile de studenți ocupate: o activitate pe grupă ocupă grupa și toate subgrupele; una pe subgrupă, doar subgrupa.</summary>
    private static IEnumerable<Guid> UnitResources(TeachingAssignment a)
    {
        if (a.SubgroupId is { } sg) { yield return sg; yield break; }
        yield return a.GroupId;
        foreach (var s in a.Group.Subgroups) yield return s.Id;
    }
}
