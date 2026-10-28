using System.Text.RegularExpressions;
using SkillHub.Domain.Commons;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Domain.ValueObjects;

public class PhoneNumber : ValueObject
{
    private static readonly Regex PhoneRegex = new(@"^\+?[0-9]{7,15}$", RegexOptions.Compiled);

    public string CountryCode { get; }
    public string Number { get; }

    protected PhoneNumber()
    {
        CountryCode = string.Empty;
        Number = string.Empty;
    }

    public PhoneNumber(string countryCode, string number)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
            throw new DomainException("Country code is required.");

        if (string.IsNullOrWhiteSpace(number))
            throw new DomainException("Phone number is required.");

        countryCode = countryCode.Trim();
        number = number.Trim();

        var fullPhone = countryCode.StartsWith('+') ? $"{countryCode}{number}" : $"+{countryCode}{number}";
        if (!PhoneRegex.IsMatch(fullPhone))
            throw new DomainException($"Phone number '{fullPhone}' is invalid.");

        CountryCode = countryCode;
        Number = number;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CountryCode;
        yield return Number;
    }

    public override string ToString() => $"{CountryCode} {Number}";
}
