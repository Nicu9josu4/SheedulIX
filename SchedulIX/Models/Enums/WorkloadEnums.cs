namespace SchedulIX.Models.Enums
{
    public enum ActivityType
    {
        Course = 1,
        Seminar = 2,
        Laboratory = 3,
        Project = 4
    }

    public enum EmploymentType
    {
        Titular = 1,
        Associate = 2,   // cadru didactic asociat (cumul)
        HourlyPaid = 3   // plata cu ora
    }

    public enum StaffingPlanStatus
    {
        Draft = 0,
        Submitted = 1,
        DepartmentApproved = 2,
        FacultyApproved = 3,
        Approved = 4,
        Rejected = 5
    }

    public enum ContractType
    {
        FixedTerm = 1,
        Associate = 2,
        HourlyPaid = 3
    }

    public enum ContractStatus
    {
        Draft = 0,
        Active = 1,
        Expired = 2,
        Terminated = 3
    }

    public enum DocumentType
    {
        CurriculumVitae = 1,
        IdentityCard = 2,
        Diploma = 3,
        EmployerCertificate = 4, // adeverinta de la angajatorul de baza
        CriminalRecord = 5,
        MedicalCertificate = 6,
        Other = 99
    }

    public enum DocumentStatus
    {
        Missing = 0,
        Submitted = 1,
        Validated = 2,
        Rejected = 3
    }
}
