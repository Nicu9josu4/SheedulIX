namespace SchedulIX.Models.Entities
{
    public class TimeSlot
    {
        public int SlotNumber { get; set; } // 1, 2, 3, 4, 5
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsLunchBreak { get; set; }

        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}
