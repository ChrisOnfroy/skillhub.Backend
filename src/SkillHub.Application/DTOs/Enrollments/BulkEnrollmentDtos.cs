namespace SkillHub.Application.DTOs.Enrollments;

public record BulkEnrollmentRowDto(
    int RowNumber,
    string? ParticipantEmail,
    string? WorkshopCode);

public record BulkEnrollmentRejectedRowDto(
    int RowNumber,
    string? ParticipantEmail,
    string? WorkshopCode,
    string Reason);

public record BulkEnrollmentResponseDto(
    int EnrolledCount,
    int RejectedCount,
    IReadOnlyList<BulkEnrollmentRejectedRowDto> RejectedRows);
