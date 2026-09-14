namespace SchedulIX.Models.Entities
{
    public class AcademicGroup
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // ex: "CR-211"
        public int EducationFormId { get; set; }
        public int? SeriesId { get; set; }

        public virtual EducationForm EducationForm { get; set; } = null!;
        public virtual Series? Series { get; set; }
        public virtual ICollection<Subgroup> Subgroups { get; set; } = new List<Subgroup>();
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}
