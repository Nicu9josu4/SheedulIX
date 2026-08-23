namespace SchedulIX.Models.Entities
{
    public class Series
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // ex: "Seria 1 - Anul 1"

        public virtual ICollection<AcademicGroup> Groups { get; set; } = new List<AcademicGroup>();
    }
}
