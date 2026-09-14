namespace SchedulIX.Models
{
    public record ScheduleDto(Guid Id, string Title, DateTime CreatedAt, List<ScheduleItemDto> Items, int HardConstraintViolations, int SoftConstraintScore);
    public record ScheduleItemDto(string SubjectName, string TeacherName, string GroupName, string RoomName, DayOfWeek Day, TimeSpan StartTime, TimeSpan EndTime);
    public record GenerateScheduleRequestDto(List<string> GroupIds, bool AllowSoftConstraintViolations); // int? AcademicYearId

    public record ExportScheduleDto(
        int Id,
        string DisciplineName,
        string TeacherName,
        string RoomName,
        string GroupName,
        int DayOfWeek,
        TimeSpan StartTime,
        TimeSpan EndTime,
        string ClassType,
        int WeekType
    );
}

