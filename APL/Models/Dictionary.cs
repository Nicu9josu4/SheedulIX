using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademicManagement.Domain;

public enum EmploymentType { Head, InternalMultiJob, ExternalMultiJob }
public enum ActivityType { Lecture, Seminar, Laboratory }
public enum AssignmentStatus { Draft, Assigned, Approved, Cancelled, Archived }

public class Person
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Patronymic { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string AcademicTitle { get; set; } = string.Empty;
    public string AcademicDegree { get; set; } = string.Empty;
    public bool Active { get; set; } = true;

    // Navigation Properties
    public ICollection<DepartmentEmployment> Employments { get; set; } = new List<DepartmentEmployment>();
    public ProfessorProfile? ProfessorProfile { get; set; }
}

public class Department
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Active { get; set; } = true;

    public ICollection<DepartmentEmployment> Employments { get; set; } = new List<DepartmentEmployment>();
}

public class DepartmentEmployment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PersonId { get; set; }
    public Person Person { get; set; } = null!;
    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public EmploymentType EmploymentType { get; set; }
    public string Position { get; set; } = string.Empty;
    public decimal TeachingLoad { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool Active { get; set; } = true;
}

public class ProfessorProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PersonId { get; set; }
    public Person Person { get; set; } = null!;

    public ICollection<ProfessorLanguage> Languages { get; set; } = new List<ProfessorLanguage>();
    public ICollection<ProfessorSubject> Subjects { get; set; } = new List<ProfessorSubject>();
    public ICollection<TeachingAssignment> Assignments { get; set; } = new List<TeachingAssignment>();
}

public class Language
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty; // RO, RU, EN
    public string Name { get; set; } = string.Empty;
}

public class ProfessorLanguage
{
    public Guid ProfessorProfileId { get; set; }
    public ProfessorProfile ProfessorProfile { get; set; } = null!;
    public Guid LanguageId { get; set; }
    public Language Language { get; set; } = null!;
}

public class Subject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Credits { get; set; }
    public int TotalHours { get; set; }
    public bool Active { get; set; } = true;

    public ICollection<ProfessorSubject> QualifiedProfessors { get; set; } = new List<ProfessorSubject>();
}

public class ProfessorSubject
{
    public Guid ProfessorProfileId { get; set; }
    public ProfessorProfile ProfessorProfile { get; set; } = null!;
    public Guid SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;
}

public class StudyProgram
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DegreeLevel { get; set; } = string.Empty;
    public int DurationYears { get; set; }

    public ICollection<Curriculum> Curriculums { get; set; } = new List<Curriculum>();
    public ICollection<Group> Groups { get; set; } = new List<Group>();
}

public class Curriculum
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StudyProgramId { get; set; }
    public StudyProgram StudyProgram { get; set; } = null!;
    public string Version { get; set; } = string.Empty; // e.g., "2026-2027"
    public bool IsActive { get; set; } = true;

    public ICollection<CurriculumSubject> Subjects { get; set; } = new List<CurriculumSubject>();
}

public class CurriculumSubject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CurriculumId { get; set; }
    public Curriculum Curriculum { get; set; } = null!;
    public Guid SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;
    public Guid LanguageId { get; set; }
    public Language Language { get; set; } = null!;
    public int YearOfStudy { get; set; }
    public int Semester { get; set; }
    public int LectureHours { get; set; }
    public int SeminarHours { get; set; }
    public int LabHours { get; set; }
}

public class Group
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public Guid StudyProgramId { get; set; }
    public StudyProgram StudyProgram { get; set; } = null!;
    public Guid CurriculumId { get; set; }
    public Curriculum Curriculum { get; set; } = null!;
    public Guid LanguageId { get; set; }
    public Language Language { get; set; } = null!;
    public int YearOfStudy { get; set; }
    public int StudentCount { get; set; }

    public ICollection<Subgroup> Subgroups { get; set; } = new List<Subgroup>();
}

public class Subgroup
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;
    public string Name { get; set; } = string.Empty; // e.g., "SG1", "SG2"
    public int StudentCount { get; set; }
}

public class TeachingAssignment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProfessorProfileId { get; set; }
    public ProfessorProfile ProfessorProfile { get; set; } = null!;
    public Guid SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;
    public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;
    public Guid? SubgroupId { get; set; }
    public Subgroup? Subgroup { get; set; }
    public Guid LanguageId { get; set; }
    public Language Language { get; set; } = null!;
    public ActivityType ActivityType { get; set; }
    public int Hours { get; set; }
    public int Semester { get; set; }
    public AssignmentStatus Status { get; set; } = AssignmentStatus.Assigned;
}