using Microsoft.EntityFrameworkCore;
using SkillHub.Application.Interfaces.Repositories;
using SkillHub.Domain.Entities;
using SkillHub.Infrastructure.Context;

namespace SkillHub.Infrastructure.Repositories;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly ApplicationDbContext _context;

    public EnrollmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Enrollment?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .Include(e => e.Participant)
            .Include(e => e.Workshop)
                .ThenInclude(w => w.Instructor)
            .Include(e => e.CancelledByUser)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Enrollment>> GetByParticipantIdAsync(Guid participantId, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .Include(e => e.Participant)
            .Include(e => e.Workshop)
                .ThenInclude(w => w.Instructor)
            .Where(e => e.ParticipantId == participantId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Enrollment>> GetByWorkshopIdAsync(Guid workshopId, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .Include(e => e.Participant)
            .Include(e => e.CancelledByUser)
            .Where(e => e.WorkshopId == workshopId)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Enrollment enrollment, CancellationToken cancellationToken = default)
    {
        await _context.Enrollments.AddAsync(enrollment, cancellationToken);
    }
}
