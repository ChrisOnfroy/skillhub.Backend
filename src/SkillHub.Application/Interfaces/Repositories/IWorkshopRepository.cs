using SkillHub.Domain.Entities;

namespace SkillHub.Application.Interfaces.Repositories;

public interface IWorkshopRepository
{
    Task<Workshop?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Workshop?> GetByCodeWithDetailsAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Workshop>> GetUpcomingCatalogAsync(DateTime currentUtcTime, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Workshop>> GetActiveWorkshopsByParticipantAsync(Guid participantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Workshop>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Workshop workshop, CancellationToken cancellationToken = default);
}
