namespace SkillHub.Application.DTOs.Workshops;

public record TopWorkshopReportItemDto(
    Guid WorkshopId,
    string Code,
    string Title,
    string InstructorName,
    string InstructorEmail,
    DateTime StartAt,
    DateTime EndAt,
    int MaxCapacity,
    int ActiveEnrollmentsCount,
    int AvailableSpots,
    double OccupancyPercentage);
