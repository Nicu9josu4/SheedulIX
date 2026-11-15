namespace SchedulIX.Models.Entities
{
    /// <summary>
    /// Norma didactica pe functie si an universitar. Valorile sunt configurabile
    /// (nu sunt hardcodate), pentru a urma legislatia / regulamentul universitatii.
    /// </summary>
    public class PositionNorm
    {
        public int Id { get; set; }
        public int AcademicPositionId { get; set; }
        public string AcademicYear { get; set; } = null!; // ex: "2025-2026"

        /// <summary>Norma de baza, in ore conventionale pe saptamana.</summary>
        public decimal BaseNormHours { get; set; }
        /// <summary>Limita minima de ore conventionale pe saptamana.</summary>
        public decimal MinHours { get; set; }
        /// <summary>Limita maxima (norma de baza + ore suplimentare permise).</summary>
        public decimal MaxHours { get; set; }

        public virtual AcademicPosition AcademicPosition { get; set; } = null!;
    }
}
