using SchedulIX.Models.Enums;

namespace SchedulIX.Models.Entities
{
    /// <summary>Istoricul tranzitiilor fluxului de aprobare.</summary>
    public class ApprovalStep
    {
        public int Id { get; set; }
        public int StaffingPlanId { get; set; }
        public StaffingPlanStatus FromStatus { get; set; }
        public StaffingPlanStatus ToStatus { get; set; }
        public string ActorName { get; set; } = null!;
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual StaffingPlan StaffingPlan { get; set; } = null!;
    }
}
