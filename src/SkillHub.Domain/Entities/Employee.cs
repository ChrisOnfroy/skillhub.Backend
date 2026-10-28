using SkillHub.Domain.Commons;
using SkillHub.Domain.Enums;
using SkillHub.Domain.Exceptions;
using SkillHub.Domain.ValueObjects;

namespace SkillHub.Domain.Entities;

public class Employee : User
{
    public string Specialty { get; set; } = string.Empty;
    public ICollection<Workshop> InstructedWorkshops { get; set; } = new List<Workshop>();

    protected Employee()
    {
        Role = UserRole.Employee;
    }

    public Employee(
        string firstName,
        string lastName,
        Email email,
        IdentificationDocument identification,
        string specialty,
        PhoneNumber? phoneNumber = null,
        Address? address = null)
        : base(firstName, lastName, email, identification, UserRole.Employee, phoneNumber, address)
    {
        if (string.IsNullOrWhiteSpace(specialty))
            throw new DomainException("Specialty is required for instructors/employees.");

        Specialty = specialty.Trim();
    }
}
