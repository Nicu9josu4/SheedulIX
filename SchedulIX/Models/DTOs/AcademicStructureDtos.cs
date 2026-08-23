namespace SchedulIX.Models.DTOs
{
    public record AcademicGroupDto(
        int Id,
        string Name,
        int EducationFormId,
        int? SeriesId
    );

    public record SubgroupDto(
        int Id,
        int GroupId,
        string Name,
        int StudentCount
    );

    public record CreateAcademicGroupRequest(
        string Name,
        int EducationFormId,
        int? SeriesId,
        List<CreateSubgroupRequest>? Subgroups
    );

    public record CreateSubgroupRequest(
        string Name,
        int StudentCount
    );

    public record SeriesDto(
        int Id,
        string Name
    );

    public record CreateSeriesRequest(
        string Name
    );

    public record EducationFormDto(
        int Id,
        string Name,
        int Year,
        int Semester
    );

    public record CreateEducationFormRequest(
        string Name,
        int Year,
        int Semester
    );
}
