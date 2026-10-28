using SkillHub.Domain.Commons;
using SkillHub.Domain.Enums;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Domain.Entities;

public class Workshop : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid InstructorId { get; set; }
    public Employee Instructor { get; set; } = null!;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public int MaxCapacity { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public int ActiveEnrollmentsCount => Enrollments.Count(e => e.Status == EnrollmentStatus.Active);
    public int AvailableSpots => Math.Max(0, MaxCapacity - ActiveEnrollmentsCount);

    protected Workshop()
    {
    }

    public Workshop(
        string code,
        string title,
        string description,
        Guid instructorId,
        DateTime startAt,
        DateTime endAt,
        int maxCapacity,
        DateTime currentUtcTime)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Workshop code is required.");

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Workshop title is required.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Workshop description is required.");

        if (instructorId == Guid.Empty)
            throw new DomainException("Workshop must have an assigned instructor.");

        if (startAt <= currentUtcTime)
            throw new DomainException("Workshop must be scheduled for a future date and time.");

        if (endAt <= startAt)
            throw new DomainException("End date and time must be after start date and time.");

        if (maxCapacity <= 0)
            throw new DomainException("Maximum capacity must be greater than zero.");

        Code = code.Trim().ToUpperInvariant();
        Title = title.Trim();
        Description = description.Trim();
        InstructorId = instructorId;
        StartAt = startAt;
        EndAt = endAt;
        MaxCapacity = maxCapacity;
    }

    public bool HasStarted(DateTime currentUtcTime) => currentUtcTime >= StartAt;

    public bool OverlapsWith(DateTime otherStartAt, DateTime otherEndAt) =>
        StartAt < otherEndAt && EndAt > otherStartAt;

    public Enrollment EnrollParticipant(
        Participant participant,
        IEnumerable<Workshop> participantActiveWorkshops,
        DateTime currentUtcTime)
    {
        if (participant is null)
            throw new DomainException("Participant is required.");

        // Rule 4: Future workshops only
        if (HasStarted(currentUtcTime))
            throw new DomainException("Cannot enroll in a workshop that has already started.");

        // Rule 2 & 6: No duplicate active enrollments (allows re-enrollment if previous is Cancelled)
        if (Enrollments.Any(e => e.ParticipantId == participant.Id && e.Status == EnrollmentStatus.Active))
            throw new DomainException("Participant already has an active enrollment in this workshop.");

        // Rule 1: Capacity limit (only active enrollments count)
        if (AvailableSpots <= 0)
            throw new DomainException("The workshop has no available spots.");

        // Rule 3: No schedule conflicts with participant's other active workshops
        var conflictingWorkshop = participantActiveWorkshops
            .FirstOrDefault(w => w.Id != Id && w.OverlapsWith(StartAt, EndAt));

        if (conflictingWorkshop is not null)
            throw new DomainException($"Workshop schedule conflicts with workshop '{conflictingWorkshop.Code} - {conflictingWorkshop.Title}' where the participant already has an active enrollment.");

        var enrollment = new Enrollment(participant.Id, Id);
        Enrollments.Add(enrollment);
        return enrollment;
    }
}
