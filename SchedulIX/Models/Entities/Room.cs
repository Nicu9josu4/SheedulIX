namespace SchedulIX.Models.Entities
{
    public class Room
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = null!; // ex: "201", "Bloc B-102"
        public int Capacity { get; set; } // Number of seats
        public int RoomTypeId { get; set; }
        public bool IsAvailable { get; set; } = true; // Status: repair/unavailable

        public virtual RoomType RoomType { get; set; } = null!;
        public virtual ICollection<RoomAvailability> Availabilities { get; set; } = new List<RoomAvailability>();
        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
        public virtual ICollection<TeacherPreference> TeacherPreferences { get; set; } = new List<TeacherPreference>();
    }
}
