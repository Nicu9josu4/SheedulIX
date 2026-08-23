namespace SchedulIX.Models.Entities
{
    public class TeacherPreference
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public int? DisciplineId { get; set; }
        public TimeSpan? PreferredStartTime { get; set; } // ex: 08:00 (only morning)
        public TimeSpan? PreferredEndTime { get; set; } // ex: 14:00
        public int? MandatoryRoomId { get; set; } // ex: Room 201 for specific labs

        public virtual Teacher Teacher { get; set; } = null!;
        public virtual Discipline? Discipline { get; set; }
        public virtual Room? MandatoryRoom { get; set; }
    }
}
