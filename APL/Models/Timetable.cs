namespace AcademicManagement.Domain;

/// <summary>Enum-urile se serializează ca text în API („Lecture”, „Odd” etc.).</summary>
[System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
public enum RoomType { Lecture, Seminar, Laboratory }
[System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
public enum WeekParity { All, Odd, Even }

/// <summary>Sală de curs, seminar sau laborator, într-un corp (clădire).</summary>
public class Room
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;       // ex. 3-201
    public string Building { get; set; } = string.Empty;   // corp / clădire, ex. "Corp 3"
    public int Capacity { get; set; }
    public RoomType Type { get; set; }
    public bool HasComputers { get; set; }
    public bool Active { get; set; } = true;               // false = în reparație / indisponibilă
}

/// <summary>O generare de orar pentru un an universitar și un semestru. Orarul curent este ultima rulare.</summary>
public class TimetableRun
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string AcademicYear { get; set; } = string.Empty;
    public int Semester { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string SolverStatus { get; set; } = string.Empty;
    public int SessionsTotal { get; set; }
    public int SessionsPlaced { get; set; }
    public int Penalty { get; set; }
    public int TimeLimitSeconds { get; set; }
    public double SolveSeconds { get; set; }

    public ICollection<TimetableEntry> Entries { get; set; } = new List<TimetableEntry>();
    public ICollection<TimetableUnplaced> Unplaced { get; set; } = new List<TimetableUnplaced>();
}

/// <summary>O pereche plasată: o activitate repartizată, într-o zi, la o pereche, într-o sală, cu o paritate a săptămânii.</summary>
public class TimetableEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TimetableRunId { get; set; }
    public TimetableRun TimetableRun { get; set; } = null!;
    public Guid TeachingAssignmentId { get; set; }
    public TeachingAssignment TeachingAssignment { get; set; } = null!;
    public int DayOfWeek { get; set; }     // 1 = luni ... 6 = sâmbătă
    public int SlotNumber { get; set; }
    public Guid RoomId { get; set; }
    public Room Room { get; set; } = null!;
    public WeekParity Parity { get; set; }
}

/// <summary>Perechi care nu au putut fi plasate, cu motivul.</summary>
public class TimetableUnplaced
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TimetableRunId { get; set; }
    public TimetableRun TimetableRun { get; set; } = null!;
    public Guid TeachingAssignmentId { get; set; }
    public TeachingAssignment TeachingAssignment { get; set; } = null!;
    public int Sessions { get; set; }
    public string Reason { get; set; } = string.Empty;
}
