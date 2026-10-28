using Microsoft.Extensions.Caching.Memory;
using SkillHub.Application.Interfaces.Auth;

namespace SkillHub.Infrastructure.Services;

public class MemoryPasswordResetCodeStore : IPasswordResetCodeStore
{
    private readonly IMemoryCache _cache;

    public MemoryPasswordResetCodeStore(IMemoryCache cache)
    {
        _cache = cache;
    }

    private static string GetKey(string identificationNumber) => $"pwd_reset_{identificationNumber.Trim()}";

    public void SaveCode(string identificationNumber, PasswordResetCodeData data, TimeSpan ttl)
    {
        var key = GetKey(identificationNumber);
        _cache.Set(key, data, ttl);
    }

    public PasswordResetCodeData? GetCode(string identificationNumber)
    {
        var key = GetKey(identificationNumber);
        return _cache.TryGetValue(key, out PasswordResetCodeData? data) ? data : null;
    }

    public void RemoveCode(string identificationNumber)
    {
        var key = GetKey(identificationNumber);
        _cache.Remove(key);
    }
}
