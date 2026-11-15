using SchedulIX.Models.Enums;

namespace SchedulIX.Models.Entities
{
    /// <summary>Repartizarea unei activitati didactice catre un cadru didactic.</summary>
    public class TeachingAssignment
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public int DisciplineId { get; set; }
        public int? StaffingPlanId { get; set; }

        public string AcademicYear { get; set; } = null!;
        public int Semester { get; set; }
        public ActivityType ActivityType { get; set; }

        // Formatia de studiu (cel mult una dintre ele)
        public int? SeriesId { get; set; }
        public int? GroupId { get; set; }
        public int? SubgroupId { get; set; }

        public decimal WeeklyHours { get; set; }
        public int Weeks { get; set; } = 14;

        public virtual Teacher Teacher { get; set; } = null!;
        public virtual Discipline Discipline { get; set; } = null!;
        public virtual StaffingPlan? StaffingPlan { get; set; }
        public virtual Series? Series { get; set; }
        public virtual AcademicGroup? Group { get; set; }
        public virtual Subgroup? Subgroup { get; set; }
    }
}
