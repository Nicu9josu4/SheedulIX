namespace SchedulIX.Models.DTOs
{
    public record DisciplineDto(
        int Id,
        string Name,
        int CourseHours,
        int SeminarHours,
        int LabHours
    );

    public record CreateDisciplineRequest(
        string Name,
        int CourseHours = 2,
        int SeminarHours = 2,
        int LabHours = 2
    );
}
