namespace SchedulIX.Models.Entities
{
    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;

        public int GroupId { get; set; }
        public int? SubgroupId { get; set; }

        public virtual AcademicGroup Group { get; set; } = null!;
        public virtual Subgroup? Subgroup { get; set; }
    }
}
