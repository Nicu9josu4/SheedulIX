namespace SchedulIX.Models.Entities
{
    public class RoomAvailability
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int DayOfWeek { get; set; } // 1 = Monday, ..., 6 = Saturday
        public TimeSpan StartTime { get; set; } // ex: 08:00
        public TimeSpan EndTime { get; set; } // ex: 12:00

        public virtual Room Room { get; set; } = null!;
    }
}
