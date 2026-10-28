namespace SkillHub.Application.Interfaces.Auth;

public class PasswordResetCodeData
{
    public string IdentificationNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string IdentityToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public int FailedAttempts { get; set; }
}

public interface IPasswordResetCodeStore
{
    void SaveCode(string identificationNumber, PasswordResetCodeData data, TimeSpan ttl);
    PasswordResetCodeData? GetCode(string identificationNumber);
    void RemoveCode(string identificationNumber);
}
