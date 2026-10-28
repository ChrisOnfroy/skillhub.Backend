namespace SkillHub.Application.DTOs.Auth;

public record LoginRequestDto(
    string Email,
    string Password);
