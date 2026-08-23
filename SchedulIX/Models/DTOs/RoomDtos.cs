namespace SchedulIX.Models.DTOs
{
    public record RoomTypeDto(int Id, string Name, bool HasComputers);

    public record RoomDto(
        int Id,
        string RoomNumber,
        int Capacity,
        RoomTypeDto RoomType,
        bool IsAvailable
    );

    public record RoomAvailabilityDto(
        int Id,
        int RoomId,
        int DayOfWeek,
        TimeSpan StartTime,
        TimeSpan EndTime
    );

    public record CreateRoomRequest(
        string RoomNumber,
        int Capacity,
        int RoomTypeId,
        List<CreateRoomAvailabilityRequest> Availabilities
    );

    public record CreateRoomAvailabilityRequest(
        int DayOfWeek,
        TimeSpan StartTime,
        TimeSpan EndTime
    );

    public record RoomCalendarDto(
        int RoomId,
        string RoomNumber,
        List<RoomScheduleItemDto> ScheduledItems
    );

    public record RoomScheduleItemDto(
        string DisciplineName,
        string TeacherName,
        string GroupName,
        int DayOfWeek,
        TimeSpan StartTime,
        TimeSpan EndTime,
        string ClassType
    );
}
