using SkillHub.Domain.Commons;
using SkillHub.Domain.Enums;
using SkillHub.Domain.Exceptions;
using SkillHub.Domain.ValueObjects;

public abstract class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public IdentificationDocument Identification { get; set; } = null!;
    public Email Email { get; set; } = null!;
    public PhoneNumber? PhoneNumber { get; set; }
    public Address? Address { get; set; }
    public UserRole Role { get; protected set; }
    public bool IsActive { get; set; } = true;

    protected User()
    {
    }

    protected User(
        string firstName,
        string lastName,
        Email email,
        IdentificationDocument identification,
        UserRole role,
        PhoneNumber? phoneNumber = null,
        Address? address = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("El apellido es obligatorio.");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email ?? throw new DomainException("El correo electrónico es obligatorio.");
        Identification = identification ?? throw new DomainException("El documento de identificación es obligatorio.");
        Role = role;
        PhoneNumber = phoneNumber;
        Address = address;
        IsActive = true;
    }
}
