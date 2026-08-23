namespace SchedulIX.Models.DTOs
{
    public record TeacherDto(
        int Id,
        string FirstName,
        string LastName,
        string Email
    );

    public record CreateTeacherRequest(
        string FirstName,
        string LastName,
        string Email
    );

    public record TeacherPreferenceDto(
        int Id,
        int TeacherId,
        int? DisciplineId,
        TimeSpan? PreferredStartTime,
        TimeSpan? PreferredEndTime,
        int? MandatoryRoomId
    );

    public record CreateTeacherPreferenceRequest(
        int TeacherId,
        int? DisciplineId,
        TimeSpan? PreferredStartTime,
        TimeSpan? PreferredEndTime,
        int? MandatoryRoomId
    );
}
