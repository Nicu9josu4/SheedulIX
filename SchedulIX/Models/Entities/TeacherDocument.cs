using SchedulIX.Models.Enums;

namespace SchedulIX.Models.Entities
{
    /// <summary>Document colectat pentru un cadru didactic (optional legat de un contract).</summary>
    public class TeacherDocument
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public int? ContractId { get; set; }
        public DocumentType Type { get; set; }
        public DocumentStatus Status { get; set; } = DocumentStatus.Missing;
        public string? FileName { get; set; }
        public string? StoragePath { get; set; }
        public DateTime? UploadedAt { get; set; }
        public DateTime? ValidatedAt { get; set; }
        public string? ValidatedBy { get; set; }

        public virtual Teacher Teacher { get; set; } = null!;
        public virtual Contract? Contract { get; set; }
    }
}
