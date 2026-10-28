namespace SkillHub.Application.DTOs.Workshops;

public record WorkshopResponseDto(
    Guid Id,
    string Code,
    string Title,
    string Description,
    Guid InstructorId,
    string InstructorName,
    string InstructorEmail,
    string InstructorSpecialty,
    DateTime StartAt,
    DateTime EndAt,
    int MaxCapacity,
    int ActiveEnrollmentsCount,
    int AvailableSpots);
