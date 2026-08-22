namespace SchedulIX.Models
{
    public record ScheduleDto(Guid Id, string Name, DateTime CreatedAt, List<ScheduleItemDto> Items, int HardConstraintViolations, int SoftConstraintScore);
    public record ScheduleItemDto(string SubjectName, string TeacherName, string GroupName, string RoomName, DayOfWeek Day, TimeSpan StartTime, TimeSpan EndTime);
    public record GenerateScheduleRequestDto(Guid AcademicYearId, List<Guid> GroupIds, bool AllowSoftConstraintViolations);
}
