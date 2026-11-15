using SchedulIX.Models.Enums;

namespace SchedulIX.Models.Entities
{
    /// <summary>Contract de munca pe perioada determinata / cumul.</summary>
    public class Contract
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public string Number { get; set; } = null!;
        public ContractType Type { get; set; }
        public ContractStatus Status { get; set; } = ContractStatus.Draft;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal WeeklyHours { get; set; }
        /// <summary>Angajatorul de baza (pentru cadrele asociate).</summary>
        public string? BaseEmployer { get; set; }

        public virtual Teacher Teacher { get; set; } = null!;
        public virtual ICollection<TeacherDocument> Documents { get; set; } = new List<TeacherDocument>();
    }
}
