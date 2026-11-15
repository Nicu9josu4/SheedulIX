using SchedulIX.Models.Enums;

namespace SchedulIX.Models.Entities
{
    /// <summary>Coeficient de transformare a orelor fizice in ore conventionale.</summary>
    public class ConversionCoefficient
    {
        public int Id { get; set; }
        public string AcademicYear { get; set; } = null!;
        public ActivityType ActivityType { get; set; }
        /// <summary>Null = se aplica tuturor formelor de invatamant.</summary>
        public int? EducationFormId { get; set; }
        public decimal Coefficient { get; set; } = 1m;

        public virtual EducationForm? EducationForm { get; set; }
    }
}
