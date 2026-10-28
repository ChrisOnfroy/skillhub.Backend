namespace SkillHub.Application.DTOs.Auth;

public record AuthResponseDto(
    string Token,
    Guid UserId,
    string FullName,
    string Email,
    string Role,
    DateTime ExpiresAt);
