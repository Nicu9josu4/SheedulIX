namespace SchedulIX.Models.Entities
{
    public class RoomType
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // Curs, Laborator, Seminar
        public bool HasComputers { get; set; }

        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}
