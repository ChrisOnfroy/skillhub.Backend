using SkillHub.Domain.Commons;
using SkillHub.Domain.Enums;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Domain.ValueObjects;

public class IdentificationDocument : ValueObject
{
    public IdentificationType Type { get; }
    
    public string Number { get; }
    
    protected IdentificationDocument()
    {
        Type = IdentificationType.NationalIdentity;
        Number = string.Empty;
    }

    public IdentificationDocument(IdentificationType type, string number)
    {
        if (!Enum.IsDefined(type))
            throw new DomainException("Invalid identification document type.");

        if (string.IsNullOrWhiteSpace(number))
            throw new DomainException("Identification document number cannot be empty.");

        Type = type;
        Number = number.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Type;
        yield return Number;
    }

    public override string ToString() => $"{Type}: {Number}";
}
