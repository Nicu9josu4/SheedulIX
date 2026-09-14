namespace SchedulIX.Models.Entities
{
    public class EducationForm
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // ex: "Full-Time", "Part-Time"

        public virtual ICollection<AcademicGroup> Groups { get; set; } = new List<AcademicGroup>();
    }
}
