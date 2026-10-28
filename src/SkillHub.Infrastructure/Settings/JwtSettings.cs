namespace SkillHub.Infrastructure.Settings;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string SecretKey { get; set; } = "SkillHubSuperSecretKeyForJwtAuthentication2026SecureKey!";
    public string Issuer { get; set; } = "SkillHub";
    public string Audience { get; set; } = "SkillHubApp";
    public int ExpiryMinutes { get; set; } = 120;
}
