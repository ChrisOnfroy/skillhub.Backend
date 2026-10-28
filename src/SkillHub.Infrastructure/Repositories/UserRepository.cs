using Microsoft.EntityFrameworkCore;
using SkillHub.Application.Interfaces.Repositories;
using SkillHub.Domain.Commons;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Enums;
using SkillHub.Infrastructure.Context;

namespace SkillHub.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.DomainUsers.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await _context.DomainUsers.FirstOrDefaultAsync(u => u.Email.Value == normalized, cancellationToken);
    }

    public async Task<Participant?> GetParticipantByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Participants.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Participant?> GetParticipantByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await _context.Participants.FirstOrDefaultAsync(p => p.Email.Value == normalized, cancellationToken);
    }

    public async Task<Employee?> GetEmployeeByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await _context.Employees.FirstOrDefaultAsync(e => e.Email.Value == normalized, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await _context.DomainUsers.AnyAsync(u => u.Email.Value == normalized, cancellationToken);
    }

    public async Task<bool> ExistsByIdentificationAsync(IdentificationType type, string number, CancellationToken cancellationToken = default)
    {
        var trimmed = number.Trim();
        return await _context.DomainUsers.AnyAsync(u => u.Identification.Type == type && u.Identification.Number == trimmed, cancellationToken);
    }

    public async Task<User?> GetByIdentificationNumberAsync(string identificationNumber, CancellationToken cancellationToken = default)
    {
        var trimmed = identificationNumber.Trim();
        return await _context.DomainUsers.FirstOrDefaultAsync(u => u.Identification.Number == trimmed, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.DomainUsers.AddAsync(user, cancellationToken);
    }
}
