namespace SkillHub.Application.DTOs.Auth;

public record ResetPasswordRequestDto(
    string? Email,
    string? Token,
    string? IdentificationNumber,
    string? Code,
    string NewPassword,
    string ConfirmPassword);
