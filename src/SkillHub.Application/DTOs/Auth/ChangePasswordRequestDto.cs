namespace SkillHub.Application.DTOs.Auth;

public record ChangePasswordRequestDto(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword);
