using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using SkillHub.Application.Interfaces.Services;
using SkillHub.Domain.Exceptions;
using SkillHub.Infrastructure.Settings;

namespace SkillHub.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<SmtpSettings> smtpOptions,
        ILogger<EmailService> logger)
    {
        _smtpSettings = smtpOptions.Value;
        _logger = logger;
    }

    public async Task SendPasswordResetCodeEmailAsync(
        string toEmail,
        string recipientName,
        string verificationCode,
        CancellationToken cancellationToken = default)
    {
        // Check if SMTP credentials are provided
        if (string.IsNullOrWhiteSpace(_smtpSettings.Username) || string.IsNullOrWhiteSpace(_smtpSettings.Password))
        {
            _logger.LogWarning(
                "[SMTP SIMULATED] No SMTP credentials configured in SmtpSettings.\n" +
                "Password reset code email to: {Email} ({Name})\n" +
                "Verification Code: {Code}",
                toEmail, recipientName, verificationCode);
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
            message.To.Add(new MailboxAddress(recipientName, toEmail));
            message.Subject = $"{verificationCode} es tu código de recuperación - SkillHub";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f4f6f9; margin: 0; padding: 20px; color: #333; }}
        .container {{ max-width: 540px; margin: 0 auto; background: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.05); }}
        .header {{ background: #2563eb; color: #ffffff; padding: 24px; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 24px; font-weight: 600; letter-spacing: -0.5px; }}
        .content {{ padding: 32px 24px; line-height: 1.6; font-size: 15px; text-align: center; }}
        .code-container {{ margin: 28px 0; }}
        .code-box {{ display: inline-block; background: #f1f5f9; border: 2px dashed #2563eb; border-radius: 8px; padding: 14px 28px; font-size: 32px; font-weight: 700; letter-spacing: 8px; color: #1e40af; font-family: monospace; }}
        .expiry-note {{ font-size: 13px; color: #64748b; margin-top: 8px; }}
        .footer {{ background: #f8fafc; border-top: 1px solid #e2e8f0; padding: 20px 24px; text-align: center; font-size: 12px; color: #64748b; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>SkillHub</h1>
        </div>
        <div class='content'>
            <p style='text-align: left;'>Hola <strong>{recipientName}</strong>,</p>
            <p style='text-align: left;'>Hemos recibido una solicitud para recuperar la contraseña de tu cuenta en <strong>SkillHub</strong>.</p>
            <p style='text-align: left;'>Tu código de verificación de 6 dígitos es:</p>
            <div class='code-container'>
                <div class='code-box'>{verificationCode}</div>
                <div class='expiry-note'>Este código es válido por 15 minutos</div>
            </div>
            <p style='text-align: left; font-size: 14px; color: #475569;'>Ingresa este código en la aplicación para verificar tu identidad y establecer tu nueva contraseña.</p>
            <p style='text-align: left; color: #64748b; font-size: 13px; margin-top: 24px;'><strong>Nota de seguridad:</strong> Si no realizaste esta solicitud, puedes ignorar este mensaje. Nadie puede acceder a tu cuenta sin este código.</p>
        </div>
        <div class='footer'>
            <p>© {DateTime.UtcNow.Year} SkillHub. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>",
                TextBody = $"Hola {recipientName},\n\nTu código de verificación para recuperar tu contraseña en SkillHub es: {verificationCode}\n\nEste código es válido por 15 minutos.\n\nSi no solicitaste este código, ignora este mensaje."
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            var secureOption = _smtpSettings.EnableSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.Auto;

            await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, secureOption, cancellationToken);

            if (!string.IsNullOrWhiteSpace(_smtpSettings.Username))
            {
                await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password, cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Password reset code email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset code email to {Email}. Verification Code generated: {Code}", toEmail, verificationCode);
            throw new DomainException($"Error al enviar el correo a través de Gmail ({ex.Message}). Verifica que el usuario y la contraseña de aplicación sean correctos.");
        }
    }
}
