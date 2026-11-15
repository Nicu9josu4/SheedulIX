using AcademicManagement.Domain;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace AcademicManagement.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Person> Persons => Set<Person>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<DepartmentEmployment> DepartmentEmployments => Set<DepartmentEmployment>();
    public DbSet<ProfessorProfile> ProfessorProfiles => Set<ProfessorProfile>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<StudyProgram> StudyPrograms => Set<StudyProgram>();
    public DbSet<Curriculum> Curriculums => Set<Curriculum>();
    public DbSet<CurriculumSubject> CurriculumSubjects => Set<CurriculumSubject>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<Subgroup> Subgroups => Set<Subgroup>();
    public DbSet<TeachingAssignment> TeachingAssignments => Set<TeachingAssignment>();

    // Sarcina didactica, cumul si aprobare
    // Orar
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<TimetableRun> TimetableRuns => Set<TimetableRun>();
    public DbSet<TimetableEntry> TimetableEntries => Set<TimetableEntry>();
    public DbSet<TimetableUnplaced> TimetableUnplaced => Set<TimetableUnplaced>();

    // Disponibilitate (doleanțe de timp)
    public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
    public DbSet<AvailabilityBlock> AvailabilityBlocks => Set<AvailabilityBlock>();
    public DbSet<AvailabilityPreference> AvailabilityPreferences => Set<AvailabilityPreference>();
    public DbSet<ExternalCommitment> ExternalCommitments => Set<ExternalCommitment>();

    public DbSet<WorkloadModel> WorkloadModels => Set<WorkloadModel>();
    public DbSet<PositionNorm> PositionNorms => Set<PositionNorm>();
    public DbSet<ConversionCoefficient> ConversionCoefficients => Set<ConversionCoefficient>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<EmploymentDocument> EmploymentDocuments => Set<EmploymentDocument>();
    public DbSet<StaffingPlan> StaffingPlans => Set<StaffingPlan>();
    public DbSet<ApprovalStep> ApprovalSteps => Set<ApprovalStep>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureWorkload(modelBuilder);

        // Mapări explicite pentru a potrivi schema din PostgreSQL
        modelBuilder.Entity<Person>().ToTable("Persons");
        modelBuilder.Entity<Department>().ToTable("Departments");
        modelBuilder.Entity<DepartmentEmployment>().ToTable("DepartmentEmployments");
        modelBuilder.Entity<ProfessorProfile>().ToTable("ProfessorProfiles");
        modelBuilder.Entity<Language>().ToTable("Languages");
        modelBuilder.Entity<ProfessorLanguage>().ToTable("ProfessorLanguages");
        modelBuilder.Entity<Subject>().ToTable("Subjects");
        modelBuilder.Entity<ProfessorSubject>().ToTable("ProfessorSubjects");
        modelBuilder.Entity<StudyProgram>().ToTable("StudyPrograms");
        modelBuilder.Entity<Curriculum>().ToTable("Curriculums");
        modelBuilder.Entity<CurriculumSubject>().ToTable("CurriculumSubjects");
        modelBuilder.Entity<Group>().ToTable("Groups");
        modelBuilder.Entity<Subgroup>().ToTable("Subgroups");
        modelBuilder.Entity<TeachingAssignment>().ToTable("TeachingAssignments");

        // Chei compuse (N:M)
        modelBuilder.Entity<ProfessorLanguage>()
            .HasKey(pl => new { pl.ProfessorProfileId, pl.LanguageId });

        modelBuilder.Entity<ProfessorSubject>()
            .HasKey(ps => new { ps.ProfessorProfileId, ps.SubjectId });
    }

    private static void ConfigureTimetable(ModelBuilder mb)
    {
        mb.Entity<Room>().ToTable("Rooms");
        mb.Entity<Room>().Property(r => r.Code).HasMaxLength(50);
        mb.Entity<Room>().Property(r => r.Building).HasMaxLength(100);
        mb.Entity<Room>().Property(r => r.Type).HasConversion<string>().HasMaxLength(20);
        mb.Entity<Room>().HasIndex(r => new { r.Building, r.Code }).IsUnique();
        mb.Entity<Room>().ToTable(t => t.HasCheckConstraint("CK_Rooms_Capacity", "\"Capacity\" > 0"));

        mb.Entity<TimetableRun>().ToTable("TimetableRuns");
        mb.Entity<TimetableRun>().Property(r => r.AcademicYear).HasMaxLength(9);
        mb.Entity<TimetableRun>().Property(r => r.SolverStatus).HasMaxLength(30);
        mb.Entity<TimetableRun>().HasIndex(r => new { r.AcademicYear, r.Semester });

        mb.Entity<TimetableEntry>().ToTable("TimetableEntries");
        mb.Entity<TimetableEntry>()
            .HasOne(e => e.TimetableRun).WithMany(r => r.Entries)
            .HasForeignKey(e => e.TimetableRunId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<TimetableEntry>()
            .HasOne(e => e.TeachingAssignment).WithMany()
            .HasForeignKey(e => e.TeachingAssignmentId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<TimetableEntry>()
            .HasOne(e => e.Room).WithMany()
            .HasForeignKey(e => e.RoomId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<TimetableEntry>().Property(e => e.Parity).HasConversion<string>().HasMaxLength(10);
        mb.Entity<TimetableEntry>().HasIndex(e => new { e.TimetableRunId, e.DayOfWeek, e.SlotNumber });

        mb.Entity<TimetableUnplaced>().ToTable("TimetableUnplaced");
        mb.Entity<TimetableUnplaced>()
            .HasOne(u => u.TimetableRun).WithMany(r => r.Unplaced)
            .HasForeignKey(u => u.TimetableRunId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<TimetableUnplaced>()
            .HasOne(u => u.TeachingAssignment).WithMany()
            .HasForeignKey(u => u.TeachingAssignmentId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<TimetableUnplaced>().Property(u => u.Reason).HasMaxLength(300);
    }

    private static void ConfigureAvailability(ModelBuilder mb)
    {
        ConfigureTimetable(mb);
        mb.Entity<TimeSlot>().ToTable("TimeSlots").HasKey(t => t.SlotNumber);
        mb.Entity<TimeSlot>().Property(t => t.SlotNumber).ValueGeneratedNever();
        // Grila implicită (de confirmat cu orarul real al UTM); modificabilă direct în tabel.
        mb.Entity<TimeSlot>().HasData(
            new TimeSlot { SlotNumber = 1, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(9, 30, 0) },
            new TimeSlot { SlotNumber = 2, StartTime = new TimeSpan(9, 45, 0), EndTime = new TimeSpan(11, 15, 0) },
            new TimeSlot { SlotNumber = 3, StartTime = new TimeSpan(11, 30, 0), EndTime = new TimeSpan(13, 0, 0) },
            new TimeSlot { SlotNumber = 4, StartTime = new TimeSpan(13, 0, 0), EndTime = new TimeSpan(14, 30, 0), IsLunchBreak = true },
            new TimeSlot { SlotNumber = 5, StartTime = new TimeSpan(14, 30, 0), EndTime = new TimeSpan(16, 0, 0) });

        mb.Entity<AvailabilityBlock>().ToTable("AvailabilityBlocks");
        mb.Entity<AvailabilityBlock>()
            .HasOne(b => b.ProfessorProfile).WithMany()
            .HasForeignKey(b => b.ProfessorProfileId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<AvailabilityBlock>().Property(b => b.AcademicYear).HasMaxLength(9);
        mb.Entity<AvailabilityBlock>().Property(b => b.State).HasConversion<string>().HasMaxLength(20);
        mb.Entity<AvailabilityBlock>()
            .HasIndex(b => new { b.ProfessorProfileId, b.AcademicYear, b.DayOfWeek, b.SlotNumber }).IsUnique();
        mb.Entity<AvailabilityBlock>().ToTable(t => t.HasCheckConstraint("CK_AvailabilityBlocks_Day", "\"DayOfWeek\" BETWEEN 1 AND 6"));

        mb.Entity<AvailabilityPreference>().ToTable("AvailabilityPreferences");
        mb.Entity<AvailabilityPreference>()
            .HasOne(p => p.ProfessorProfile).WithMany()
            .HasForeignKey(p => p.ProfessorProfileId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<AvailabilityPreference>().Property(p => p.AcademicYear).HasMaxLength(9);
        mb.Entity<AvailabilityPreference>().Property(p => p.Notes).HasMaxLength(2000);
        mb.Entity<AvailabilityPreference>().HasIndex(p => new { p.ProfessorProfileId, p.AcademicYear }).IsUnique();

        mb.Entity<ExternalCommitment>().ToTable("ExternalCommitments");
        mb.Entity<ExternalCommitment>()
            .HasOne(c => c.ProfessorProfile).WithMany()
            .HasForeignKey(c => c.ProfessorProfileId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<ExternalCommitment>().Property(c => c.AcademicYear).HasMaxLength(9);
        mb.Entity<ExternalCommitment>().Property(c => c.Location).HasMaxLength(200);
        mb.Entity<ExternalCommitment>().ToTable(t => t.HasCheckConstraint("CK_ExternalCommitments_Day", "\"DayOfWeek\" BETWEEN 1 AND 6"));
    }

    private static void ConfigureWorkload(ModelBuilder mb)
    {
        ConfigureAvailability(mb);

        mb.Entity<TeachingAssignment>().Property(a => a.AcademicYear).HasMaxLength(9);
        mb.Entity<TeachingAssignment>().HasIndex(a => new { a.ProfessorProfileId, a.AcademicYear, a.Semester });
        mb.Entity<TeachingAssignment>().ToTable(t =>
        {
            t.HasCheckConstraint("CK_TeachingAssignments_Hours", "\"Hours\" > 0");
            t.HasCheckConstraint("CK_TeachingAssignments_Semester", "\"Semester\" IN (1, 2)");
        });

        mb.Entity<WorkloadModel>().ToTable("WorkloadModels");
        mb.Entity<WorkloadModel>().Property(m => m.AcademicYear).HasMaxLength(9);
        mb.Entity<WorkloadModel>().Property(m => m.Name).HasMaxLength(200);
        mb.Entity<WorkloadModel>().Property(m => m.Status).HasConversion<string>().HasMaxLength(20);
        mb.Entity<WorkloadModel>().HasIndex(m => m.AcademicYear).IsUnique();

        mb.Entity<PositionNorm>().ToTable("PositionNorms");
        mb.Entity<PositionNorm>()
            .HasOne(n => n.WorkloadModel).WithMany(m => m.Norms)
            .HasForeignKey(n => n.WorkloadModelId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<PositionNorm>().Property(n => n.Position).HasMaxLength(100);
        mb.Entity<PositionNorm>().Property(n => n.BaseNormHours).HasPrecision(7, 2);
        mb.Entity<PositionNorm>().Property(n => n.MinHours).HasPrecision(7, 2);
        mb.Entity<PositionNorm>().Property(n => n.MaxHours).HasPrecision(7, 2);
        mb.Entity<PositionNorm>().HasIndex(n => new { n.WorkloadModelId, n.Position }).IsUnique();

        mb.Entity<ConversionCoefficient>().ToTable("ConversionCoefficients");
        mb.Entity<ConversionCoefficient>()
            .HasOne(c => c.WorkloadModel).WithMany(m => m.Coefficients)
            .HasForeignKey(c => c.WorkloadModelId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<ConversionCoefficient>().Property(c => c.ActivityType).HasConversion<string>().HasMaxLength(20);
        mb.Entity<ConversionCoefficient>().Property(c => c.Coefficient).HasPrecision(5, 3);
        mb.Entity<ConversionCoefficient>().HasIndex(c => new { c.WorkloadModelId, c.ActivityType }).IsUnique();

        mb.Entity<Contract>().ToTable("Contracts");
        mb.Entity<Contract>()
            .HasOne(c => c.DepartmentEmployment).WithMany()
            .HasForeignKey(c => c.DepartmentEmploymentId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<Contract>().Property(c => c.Type).HasConversion<string>().HasMaxLength(20);
        mb.Entity<Contract>().Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
        mb.Entity<Contract>().Property(c => c.AnnualHours).HasPrecision(7, 2);
        mb.Entity<Contract>().HasIndex(c => c.Number).IsUnique();
        mb.Entity<Contract>().HasIndex(c => c.EndDate);

        mb.Entity<EmploymentDocument>().ToTable("EmploymentDocuments");
        mb.Entity<EmploymentDocument>()
            .HasOne(d => d.Person).WithMany()
            .HasForeignKey(d => d.PersonId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<EmploymentDocument>()
            .HasOne(d => d.Contract).WithMany()
            .HasForeignKey(d => d.ContractId).OnDelete(DeleteBehavior.SetNull);
        mb.Entity<EmploymentDocument>().Property(d => d.Type).HasConversion<string>().HasMaxLength(30);
        mb.Entity<EmploymentDocument>().Property(d => d.Status).HasConversion<string>().HasMaxLength(20);

        mb.Entity<StaffingPlan>().ToTable("StaffingPlans");
        mb.Entity<StaffingPlan>()
            .HasOne(p => p.Department).WithMany()
            .HasForeignKey(p => p.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<StaffingPlan>().Property(p => p.AcademicYear).HasMaxLength(9);
        mb.Entity<StaffingPlan>().Property(p => p.Status).HasConversion<string>().HasMaxLength(30);
        mb.Entity<StaffingPlan>().HasIndex(p => new { p.DepartmentId, p.AcademicYear }).IsUnique();

        mb.Entity<ApprovalStep>().ToTable("ApprovalSteps");
        mb.Entity<ApprovalStep>()
            .HasOne(s => s.StaffingPlan).WithMany(p => p.ApprovalSteps)
            .HasForeignKey(s => s.StaffingPlanId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<ApprovalStep>().Property(s => s.FromStatus).HasConversion<string>().HasMaxLength(30);
        mb.Entity<ApprovalStep>().Property(s => s.ToStatus).HasConversion<string>().HasMaxLength(30);
    }
}