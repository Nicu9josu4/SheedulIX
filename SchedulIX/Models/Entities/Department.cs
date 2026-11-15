namespace SchedulIX.Models.Entities
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;

        public virtual ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
        public virtual ICollection<StaffingPlan> StaffingPlans { get; set; } = new List<StaffingPlan>();
    }
}
