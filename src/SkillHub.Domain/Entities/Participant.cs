using SkillHub.Domain.Commons;
using SkillHub.Domain.Enums;
using SkillHub.Domain.ValueObjects;

namespace SkillHub.Domain.Entities;

public class Participant : User
{
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    protected Participant()
    {
        Role = UserRole.Participant;
    }

    public Participant(
        string firstName,
        string lastName,
        Email email,
        IdentificationDocument identification,
        PhoneNumber? phoneNumber = null,
        Address? address = null)
        : base(firstName, lastName, email, identification, UserRole.Participant, phoneNumber, address)
    {
    }
}
