using SchedulIX.Models.Enums;

namespace SchedulIX.Models.Entities
{
    /// <summary>Statul de functii al unui departament pentru un an universitar.</summary>
    public class StaffingPlan
    {
        public int Id { get; set; }
        public int DepartmentId { get; set; }
        public string AcademicYear { get; set; } = null!;
        public StaffingPlanStatus Status { get; set; } = StaffingPlanStatus.Draft;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual Department Department { get; set; } = null!;
        public virtual ICollection<TeachingAssignment> Assignments { get; set; } = new List<TeachingAssignment>();
        public virtual ICollection<ApprovalStep> ApprovalSteps { get; set; } = new List<ApprovalStep>();
    }
}
