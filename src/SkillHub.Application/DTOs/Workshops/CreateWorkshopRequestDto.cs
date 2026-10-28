namespace SkillHub.Application.DTOs.Workshops;

public record CreateWorkshopRequestDto(
    string Code,
    string Title,
    string Description,
    string InstructorEmail,
    DateTime StartAt,
    DateTime EndAt,
    int MaxCapacity);
