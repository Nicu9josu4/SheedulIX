namespace SchedulIX.Models.DTOs
{
    public record StudentDto(
        int Id,
        string FirstName,
        string LastName,
        string Email,
        int GroupId,
        int? SubgroupId
    );

    public record CreateStudentRequest(
        string FirstName,
        string LastName,
        string Email,
        int GroupId,
        int? SubgroupId
    );
}
