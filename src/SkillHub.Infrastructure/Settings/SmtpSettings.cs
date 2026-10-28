namespace SkillHub.Infrastructure.Settings;

public class SmtpSettings
{
    public const string SectionName = "SmtpSettings";

    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string SenderName { get; set; } = "SkillHub Platform";
    public string SenderEmail { get; set; } = "no-reply@skillhub.com";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ResetPasswordBaseUrl { get; set; } = "http://localhost:3000/reset-password";
}
