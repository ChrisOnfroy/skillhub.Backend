namespace SkillHub.Application.DTOs.Enrollments;

public record EnrollmentResponseDto(
    Guid Id,
    Guid ParticipantId,
    string ParticipantFullName,
    string ParticipantEmail,
    Guid WorkshopId,
    string WorkshopCode,
    string WorkshopTitle,
    DateTime WorkshopStartAt,
    DateTime WorkshopEndAt,
    string Status,
    DateTime CreatedAt,
    DateTime? CancelledAt,
    Guid? CancelledByUserId);
