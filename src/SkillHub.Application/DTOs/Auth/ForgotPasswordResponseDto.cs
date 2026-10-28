namespace SkillHub.Application.DTOs.Auth;

public record ForgotPasswordResponseDto(string Message, string? MaskedEmail = null);
