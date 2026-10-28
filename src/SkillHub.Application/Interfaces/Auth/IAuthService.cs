using SkillHub.Application.DTOs.Auth;

namespace SkillHub.Application.Interfaces.Auth;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterParticipantAsync(RegisterParticipantRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken = default);
    Task<VerifyResetCodeResponseDto> VerifyResetCodeAsync(VerifyResetCodeRequestDto request, CancellationToken cancellationToken = default);
    Task<ResetPasswordResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken = default);
    Task<ResetPasswordResponseDto> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request, CancellationToken cancellationToken = default);
}
