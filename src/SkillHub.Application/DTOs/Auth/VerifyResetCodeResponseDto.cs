namespace SkillHub.Application.DTOs.Auth;

public record VerifyResetCodeResponseDto(
    bool IsValid,
    string Message,
    string Email,
    string ResetToken);
