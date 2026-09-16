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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
}