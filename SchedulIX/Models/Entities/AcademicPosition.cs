namespace SchedulIX.Models.Entities
{
    /// <summary>Functia didactica (profesor, conferentiar, lector, asistent).</summary>
    public class AcademicPosition
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Code { get; set; } = null!;

        public virtual ICollection<PositionNorm> Norms { get; set; } = new List<PositionNorm>();
        public virtual ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
    }
}
