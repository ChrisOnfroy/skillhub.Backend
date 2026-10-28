using SkillHub.Domain.Commons;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Enums;

namespace SkillHub.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Participant?> GetParticipantByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Participant?> GetParticipantByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Employee?> GetEmployeeByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByIdentificationAsync(IdentificationType type, string number, CancellationToken cancellationToken = default);
    Task<User?> GetByIdentificationNumberAsync(string identificationNumber, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
}
