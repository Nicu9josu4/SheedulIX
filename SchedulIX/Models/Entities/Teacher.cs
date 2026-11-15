using SchedulIX.Models.Enums;

namespace SchedulIX.Models.Entities
{
    public class Teacher
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;

        public int? DepartmentId { get; set; }
        public int? AcademicPositionId { get; set; }
        public EmploymentType EmploymentType { get; set; } = EmploymentType.Titular;

        public virtual Department? Department { get; set; }
        public virtual AcademicPosition? AcademicPosition { get; set; }
        public virtual ICollection<TeacherPreference> Preferences { get; set; } = new List<TeacherPreference>();
        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
        public virtual ICollection<TeachingAssignment> Assignments { get; set; } = new List<TeachingAssignment>();
        public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
        public virtual ICollection<TeacherDocument> Documents { get; set; } = new List<TeacherDocument>();
    }
}
