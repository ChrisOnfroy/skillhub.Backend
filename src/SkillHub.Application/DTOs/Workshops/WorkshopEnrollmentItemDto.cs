namespace SkillHub.Application.DTOs.Workshops;

public record WorkshopEnrollmentItemDto(
    Guid EnrollmentId,
    Guid ParticipantId,
    string ParticipantFullName,
    string ParticipantEmail,
    string Identification,
    string Status,
    DateTime EnrolledAt,
    DateTime? CancelledAt,
    Guid? CancelledByUserId,
    string? CancelledByEmail);
