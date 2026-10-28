using Microsoft.EntityFrameworkCore;
using SkillHub.Application.Interfaces.Repositories;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Enums;
using SkillHub.Infrastructure.Context;

namespace SkillHub.Infrastructure.Repositories;

public class WorkshopRepository : IWorkshopRepository
{
    private readonly ApplicationDbContext _context;

    public WorkshopRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Workshop?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Workshops
            .Include(w => w.Instructor)
            .Include(w => w.Enrollments)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<Workshop?> GetByCodeWithDetailsAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return await _context.Workshops
            .Include(w => w.Instructor)
            .Include(w => w.Enrollments)
            .FirstOrDefaultAsync(w => w.Code == normalized, cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return await _context.Workshops.AnyAsync(w => w.Code == normalized, cancellationToken);
    }

    public async Task<IReadOnlyList<Workshop>> GetUpcomingCatalogAsync(DateTime currentUtcTime, CancellationToken cancellationToken = default)
    {
        return await _context.Workshops
            .Include(w => w.Instructor)
            .Include(w => w.Enrollments)
            .Where(w => w.StartAt > currentUtcTime)
            .OrderBy(w => w.StartAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Workshop>> GetActiveWorkshopsByParticipantAsync(Guid participantId, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .Where(e => e.ParticipantId == participantId && e.Status == EnrollmentStatus.Active)
            .Select(e => e.Workshop)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Workshop>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Workshops
            .Include(w => w.Instructor)
            .Include(w => w.Enrollments)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Workshop workshop, CancellationToken cancellationToken = default)
    {
        await _context.Workshops.AddAsync(workshop, cancellationToken);
    }
}
