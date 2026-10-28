using SkillHub.Domain.Entities;

namespace SkillHub.Application.Interfaces.Repositories;

public interface IEnrollmentRepository
{
    Task<Enrollment?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Enrollment>> GetByParticipantIdAsync(Guid participantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Enrollment>> GetByWorkshopIdAsync(Guid workshopId, CancellationToken cancellationToken = default);
    Task AddAsync(Enrollment enrollment, CancellationToken cancellationToken = default);
}
