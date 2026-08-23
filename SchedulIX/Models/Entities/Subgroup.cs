namespace SchedulIX.Models.Entities
{
    public class Subgroup
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string Name { get; set; } = null!; // ex: "CR-211-1", "CR-211-2"

        public virtual AcademicGroup Group { get; set; } = null!;
        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}
