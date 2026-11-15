namespace AcademicManagement.Domain;

// ===== Enum-uri pentru sarcina didactica si cumul =====
public enum ContractType { FixedTerm, Associate, HourlyPaid }
public enum ContractStatus { Draft, Active, Expired, Terminated }
public enum DocumentType { CurriculumVitae, IdentityCard, Diploma, EmployerCertificate, CriminalRecord, MedicalCertificate, Other }
public enum DocumentStatus { Missing, Submitted, Validated, Rejected }
public enum StaffingPlanStatus { Draft, Submitted, DepartmentApproved, FacultyApproved, Approved, Rejected }

/// <summary>
/// Norma didactica pe functie si an universitar. Valorile sunt configurabile
/// (nu sunt hardcodate) ca sa urmeze legislatia / regulamentul universitatii.
/// "Position" se potriveste cu DepartmentEmployment.Position.
/// </summary>
public class PositionNorm
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Position { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty; // ex: 2026-2027
    /// <summary>Norma de baza, in ore conventionale pe saptamana.</summary>
    public decimal BaseNormHours { get; set; }
    public decimal MinHours { get; set; }
    public decimal MaxHours { get; set; }
    /// <summary>Numarul de saptamani ale semestrului, folosit la transformarea orelor semestriale in ore/saptamana.</summary>
    public int WeeksPerSemester { get; set; } = 14;
}

/// <summary>Coeficient de transformare a orelor fizice in ore conventionale.</summary>
public class ConversionCoefficient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string AcademicYear { get; set; } = string.Empty;
    public ActivityType ActivityType { get; set; }
    public decimal Coefficient { get; set; } = 1m;
}

/// <summary>Contract pe perioada determinata / cumul, legat de o angajare intr-un departament.</summary>
public class Contract
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DepartmentEmploymentId { get; set; }
    public DepartmentEmployment DepartmentEmployment { get; set; } = null!;
    public string Number { get; set; } = string.Empty;
    public ContractType Type { get; set; }
    public ContractStatus Status { get; set; } = ContractStatus.Draft;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal WeeklyHours { get; set; }
    /// <summary>Angajatorul de baza (pentru cadrele asociate externe).</summary>
    public string? BaseEmployer { get; set; }
}

/// <summary>Document colectat pentru o persoana (optional legat de un contract).</summary>
public class EmploymentDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PersonId { get; set; }
    public Person Person { get; set; } = null!;
    public Guid? ContractId { get; set; }
    public Contract? Contract { get; set; }
    public DocumentType Type { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Missing;
    public string? FileName { get; set; }
    public string? StoragePath { get; set; }
    public DateTime? UploadedAt { get; set; }
    public DateTime? ValidatedAt { get; set; }
    public string? ValidatedBy { get; set; }
}

/// <summary>Statul de functii al unui departament pentru un an universitar.</summary>
public class StaffingPlan
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public string AcademicYear { get; set; } = string.Empty;
    public StaffingPlanStatus Status { get; set; } = StaffingPlanStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<ApprovalStep> ApprovalSteps { get; set; } = new List<ApprovalStep>();
}

/// <summary>Istoricul tranzitiilor fluxului de aprobare.</summary>
public class ApprovalStep
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StaffingPlanId { get; set; }
    public StaffingPlan StaffingPlan { get; set; } = null!;
    public StaffingPlanStatus FromStatus { get; set; }
    public StaffingPlanStatus ToStatus { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
