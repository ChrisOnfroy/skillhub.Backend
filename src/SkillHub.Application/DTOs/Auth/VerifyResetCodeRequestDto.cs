namespace SkillHub.Application.DTOs.Auth;

public record VerifyResetCodeRequestDto(
    string IdentificationNumber,
    string Code);
