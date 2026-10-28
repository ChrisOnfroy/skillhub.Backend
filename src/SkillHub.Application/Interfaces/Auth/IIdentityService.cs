using SkillHub.Application.DTOs.Auth;
using SkillHub.Domain.Commons;

namespace SkillHub.Application.Interfaces.Auth;

public interface IIdentityService
{
    Task CreateCredentialsAsync(User user, string password, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> GenerateTokenForUserAsync(User user, CancellationToken cancellationToken = default);
    Task<string?> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
}
