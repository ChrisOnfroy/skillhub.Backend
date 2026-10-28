using SkillHub.Domain.Commons;
using SkillHub.Domain.Enums;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Domain.Entities;

public class Enrollment : BaseEntity
{
    public Guid ParticipantId { get; set; }
    public Participant Participant { get; set; } = null!;

    public Guid WorkshopId { get; set; }
    public Workshop Workshop { get; set; } = null!;

    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
    public DateTime? CancelledAt { get; set; }
    public Guid? CancelledByUserId { get; set; }
    public User? CancelledByUser { get; set; }

    protected Enrollment()
    {
    }

    public Enrollment(Guid participantId, Guid workshopId)
    {
        if (participantId == Guid.Empty)
            throw new DomainException("Participant is required.");

        if (workshopId == Guid.Empty)
            throw new DomainException("Workshop is required.");

        ParticipantId = participantId;
        WorkshopId = workshopId;
        Status = EnrollmentStatus.Active;
    }

    public void Cancel(Guid cancelledByUserId, bool isEmployee, DateTime currentUtcTime, DateTime workshopStartAt)
    {
        if (Status == EnrollmentStatus.Cancelled)
            throw new DomainException("Enrollment is already cancelled.");

        if (currentUtcTime >= workshopStartAt)
            throw new DomainException("Cannot cancel enrollment for a workshop that has already started.");

        if (!isEmployee && (workshopStartAt - currentUtcTime) < TimeSpan.FromHours(24))
            throw new DomainException("Participants can only cancel their enrollment at least 24 hours before the workshop starts.");

        Status = EnrollmentStatus.Cancelled;
        CancelledAt = currentUtcTime;
        CancelledByUserId = cancelledByUserId;
        SetUpdatedAt();
    }
}
