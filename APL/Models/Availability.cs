namespace AcademicManagement.Domain;

/// <summary>Se serializează ca text („Blocked”, „Avoid”, „Preferred”) în API.</summary>
[System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
public enum AvailabilityState { Blocked, Avoid, Preferred }

/// <summary>Grila orară: o pereche = interval fix (ex. 8:00–9:30). Valorile implicite sunt de confirmat cu orarul real al UTM.</summary>
public class TimeSlot
{
    public int SlotNumber { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsLunchBreak { get; set; }
}

/// <summary>O celulă a grilei săptămânale (zi × pereche) marcată de profesor. Celulele nemarcate sunt libere.</summary>
public class AvailabilityBlock
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProfessorProfileId { get; set; }
    public ProfessorProfile ProfessorProfile { get; set; } = null!;
    public string AcademicYear { get; set; } = string.Empty;
    public int DayOfWeek { get; set; }      // 1 = luni ... 6 = sâmbătă
    public int SlotNumber { get; set; }
    public AvailabilityState State { get; set; }
}

/// <summary>Preferințe generale ale profesorului pentru un an: număr maxim de zile de predare și observații.</summary>
public class AvailabilityPreference
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProfessorProfileId { get; set; }
    public ProfessorProfile ProfessorProfile { get; set; } = null!;
    public string AcademicYear { get; set; } = string.Empty;
    public int? MaxTeachingDays { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Interval în care profesorul este ocupat în altă facultate / instituție.</summary>
public class ExternalCommitment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProfessorProfileId { get; set; }
    public ProfessorProfile ProfessorProfile { get; set; } = null!;
    public string AcademicYear { get; set; } = string.Empty;
    public int DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Location { get; set; } = string.Empty;
}
