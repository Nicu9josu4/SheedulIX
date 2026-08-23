namespace SchedulIX.Models.Entities
{
    public class Schedule
    {
        public int Id { get; set; }
        public int DisciplineId { get; set; }
        public int TeacherId { get; set; }
        public int RoomId { get; set; }
        public int? GroupId { get; set; }
        public int? SubgroupId { get; set; }
        public int? SeriesId { get; set; }
        public int DayOfWeek { get; set; } // 1-6
        public int TimeSlotNumber { get; set; }
        public int WeekType { get; set; } // 0 = All, 1 = Even, 2 = Odd
        public string ClassType { get; set; } = null!; // "Course", "Seminar", "Lab"
        public DateTime CreatedAt { get; set; }

        public virtual Discipline Discipline { get; set; } = null!;
        public virtual Teacher Teacher { get; set; } = null!;
        public virtual Room Room { get; set; } = null!;
        public virtual AcademicGroup? Group { get; set; }
        public virtual Subgroup? Subgroup { get; set; }
        public virtual Series? Series { get; set; }
        public virtual TimeSlot TimeSlot { get; set; } = null!;
    }
}
