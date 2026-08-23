namespace SchedulIX.Models.Entities
{
    public class Teacher
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;

        public virtual ICollection<TeacherPreference> Preferences { get; set; } = new List<TeacherPreference>();
        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}
