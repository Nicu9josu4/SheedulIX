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

    private static void ConfigureWorkload(ModelBuilder mb)
    {
        mb.Entity<TeachingAssignment>().Property(a => a.AcademicYear).HasMaxLength(9);
        mb.Entity<TeachingAssignment>().HasIndex(a => new { a.ProfessorProfileId, a.AcademicYear, a.Semester });
        mb.Entity<TeachingAssignment>().ToTable(t =>
        {
            t.HasCheckConstraint("CK_TeachingAssignments_Hours", "\"Hours\" > 0");
            t.HasCheckConstraint("CK_TeachingAssignments_Semester", "\"Semester\" IN (1, 2)");
        });

        mb.Entity<PositionNorm>().ToTable("PositionNorms");
        mb.Entity<PositionNorm>().Property(n => n.Position).HasMaxLength(100);
        mb.Entity<PositionNorm>().Property(n => n.AcademicYear).HasMaxLength(9);
        mb.Entity<PositionNorm>().Property(n => n.BaseNormHours).HasPrecision(5, 2);
        mb.Entity<PositionNorm>().Property(n => n.MinHours).HasPrecision(5, 2);
        mb.Entity<PositionNorm>().Property(n => n.MaxHours).HasPrecision(5, 2);
        mb.Entity<PositionNorm>().HasIndex(n => new { n.Position, n.AcademicYear }).IsUnique();

        mb.Entity<ConversionCoefficient>().ToTable("ConversionCoefficients");
        mb.Entity<ConversionCoefficient>().Property(c => c.AcademicYear).HasMaxLength(9);
        mb.Entity<ConversionCoefficient>().Property(c => c.ActivityType).HasConversion<string>().HasMaxLength(20);
        mb.Entity<ConversionCoefficient>().Property(c => c.Coefficient).HasPrecision(5, 3);
        mb.Entity<ConversionCoefficient>().HasIndex(c => new { c.AcademicYear, c.ActivityType }).IsUnique();

        mb.Entity<Contract>().ToTable("Contracts");
        mb.Entity<Contract>()
            .HasOne(c => c.DepartmentEmployment).WithMany()
            .HasForeignKey(c => c.DepartmentEmploymentId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<Contract>().Property(c => c.Type).HasConversion<string>().HasMaxLength(20);
        mb.Entity<Contract>().Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
        mb.Entity<Contract>().Property(c => c.WeeklyHours).HasPrecision(5, 2);
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