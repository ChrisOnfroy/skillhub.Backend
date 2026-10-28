namespace SkillHub.Application.Interfaces.Services;

public interface IEmailService
{
    Task SendPasswordResetCodeEmailAsync(
        string toEmail,
        string recipientName,
        string verificationCode,
        CancellationToken cancellationToken = default);
}
