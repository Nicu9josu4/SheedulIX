namespace SchedulIX.Models.Entities
{
    public class Discipline
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int CourseHours { get; set; } = 2;
        public int SeminarHours { get; set; } = 2;
        public int LabHours { get; set; } = 2;

        public virtual ICollection<TeacherPreference> TeacherPreferences { get; set; } = new List<TeacherPreference>();
        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}
