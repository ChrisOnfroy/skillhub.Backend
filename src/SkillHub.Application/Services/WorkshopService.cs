using SkillHub.Application.DTOs.Workshops;
using SkillHub.Application.Interfaces.Repositories;
using SkillHub.Application.Interfaces.Services;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Application.Services;

public class WorkshopService : IWorkshopService
{
    private readonly IWorkshopRepository _workshopRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WorkshopService(
        IWorkshopRepository workshopRepository,
        IUserRepository userRepository,
        IEnrollmentRepository enrollmentRepository,
        IUnitOfWork unitOfWork)
    {
        _workshopRepository = workshopRepository;
        _userRepository = userRepository;
        _enrollmentRepository = enrollmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<WorkshopResponseDto> CreateWorkshopAsync(
        CreateWorkshopRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new DomainException("Workshop code is required.");

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        if (await _workshopRepository.ExistsByCodeAsync(normalizedCode, cancellationToken))
            throw new DomainException($"A workshop with code '{normalizedCode}' already exists.");

        if (string.IsNullOrWhiteSpace(request.InstructorEmail))
            throw new DomainException("Instructor email is required.");

        var normalizedEmail = request.InstructorEmail.Trim().ToLowerInvariant();
        var instructor = await _userRepository.GetEmployeeByEmailAsync(normalizedEmail, cancellationToken);
        if (instructor is null)
            throw new NotFoundException($"No instructor registered with email '{normalizedEmail}' was found.");

        var workshop = new Workshop(
            normalizedCode,
            request.Title,
            request.Description,
            instructor.Id,
            request.StartAt,
            request.EndAt,
            request.MaxCapacity,
            DateTime.UtcNow);

        await _workshopRepository.AddAsync(workshop, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(workshop, instructor);
    }

    public async Task<IReadOnlyList<WorkshopResponseDto>> GetAvailableCatalogAsync(
        CancellationToken cancellationToken = default)
    {
        var workshops = await _workshopRepository.GetUpcomingCatalogAsync(DateTime.UtcNow, cancellationToken);
        return workshops.Select(w => MapToResponseDto(w, w.Instructor)).ToList();
    }

    public async Task<WorkshopResponseDto> GetWorkshopByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var workshop = await _workshopRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        if (workshop is null)
            throw new NotFoundException($"Workshop with ID '{id}' was not found.");

        return MapToResponseDto(workshop, workshop.Instructor);
    }

    public async Task<IReadOnlyList<WorkshopEnrollmentItemDto>> GetWorkshopEnrollmentsAsync(
        Guid workshopId,
        CancellationToken cancellationToken = default)
    {
        var workshop = await _workshopRepository.GetByIdWithDetailsAsync(workshopId, cancellationToken);
        if (workshop is null)
            throw new NotFoundException($"Workshop with ID '{workshopId}' was not found.");

        var enrollments = await _enrollmentRepository.GetByWorkshopIdAsync(workshopId, cancellationToken);

        return enrollments.Select(e => new WorkshopEnrollmentItemDto(
            e.Id,
            e.ParticipantId,
            e.Participant.FullName,
            e.Participant.Email.Value,
            e.Participant.Identification.ToString(),
            e.Status.ToString(),
            e.CreatedAt,
            e.CancelledAt,
            e.CancelledByUserId,
            e.CancelledByUser?.Email.Value
        )).ToList();
    }

    public async Task<IReadOnlyList<TopWorkshopReportItemDto>> GetTopWorkshopsAsync(
        int top = 10,
        CancellationToken cancellationToken = default)
    {
        if (top <= 0) top = 10;

        var workshops = await _workshopRepository.GetAllWithDetailsAsync(cancellationToken);

        return workshops
            .OrderByDescending(w => w.ActiveEnrollmentsCount)
            .ThenBy(w => w.StartAt)
            .Take(top)
            .Select(w => new TopWorkshopReportItemDto(
                w.Id,
                w.Code,
                w.Title,
                w.Instructor.FullName,
                w.Instructor.Email.Value,
                w.StartAt,
                w.EndAt,
                w.MaxCapacity,
                w.ActiveEnrollmentsCount,
                w.AvailableSpots,
                w.MaxCapacity > 0
                    ? Math.Round((double)w.ActiveEnrollmentsCount / w.MaxCapacity * 100, 2)
                    : 0))
            .ToList();
    }

    private static WorkshopResponseDto MapToResponseDto(Workshop workshop, Employee instructor) =>
        new(
            workshop.Id,
            workshop.Code,
            workshop.Title,
            workshop.Description,
            instructor.Id,
            instructor.FullName,
            instructor.Email.Value,
            instructor.Specialty,
            workshop.StartAt,
            workshop.EndAt,
            workshop.MaxCapacity,
            workshop.ActiveEnrollmentsCount,
            workshop.AvailableSpots);
}
