using SkillHub.Application.DTOs.Enrollments;
using SkillHub.Application.Interfaces.Repositories;
using SkillHub.Application.Interfaces.Services;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Application.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IWorkshopRepository _workshopRepository;
    private readonly IUserRepository _userRepository;
    private readonly IExcelEnrollmentReader _excelReader;
    private readonly IPdfCertificateGenerator _pdfGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public EnrollmentService(
        IEnrollmentRepository enrollmentRepository,
        IWorkshopRepository workshopRepository,
        IUserRepository userRepository,
        IExcelEnrollmentReader excelReader,
        IPdfCertificateGenerator pdfGenerator,
        IUnitOfWork unitOfWork)
    {
        _enrollmentRepository = enrollmentRepository;
        _workshopRepository = workshopRepository;
        _userRepository = userRepository;
        _excelReader = excelReader;
        _pdfGenerator = pdfGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<EnrollmentResponseDto> EnrollAsync(
        Guid participantId,
        CreateEnrollmentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var participant = await _userRepository.GetParticipantByIdAsync(participantId, cancellationToken);
        if (participant is null)
            throw new NotFoundException("Authenticated participant does not exist.");

        var workshop = await _workshopRepository.GetByIdWithDetailsAsync(request.WorkshopId, cancellationToken);
        if (workshop is null)
            throw new NotFoundException($"Workshop with ID '{request.WorkshopId}' was not found.");

        var activeWorkshops = await _workshopRepository.GetActiveWorkshopsByParticipantAsync(participantId, cancellationToken);

        var enrollment = workshop.EnrollParticipant(participant, activeWorkshops, DateTime.UtcNow);

        await _enrollmentRepository.AddAsync(enrollment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(enrollment, participant, workshop);
    }

    public async Task<IReadOnlyList<EnrollmentResponseDto>> GetMyEnrollmentsAsync(
        Guid participantId,
        CancellationToken cancellationToken = default)
    {
        var enrollments = await _enrollmentRepository.GetByParticipantIdAsync(participantId, cancellationToken);
        return enrollments.Select(e => MapToResponseDto(e, e.Participant, e.Workshop)).ToList();
    }

    public async Task<EnrollmentResponseDto> CancelEnrollmentAsync(
        Guid enrollmentId,
        Guid currentUserId,
        bool isEmployee,
        CancellationToken cancellationToken = default)
    {
        var enrollment = await _enrollmentRepository.GetByIdWithDetailsAsync(enrollmentId, cancellationToken);
        if (enrollment is null)
            throw new NotFoundException($"Enrollment with ID '{enrollmentId}' was not found.");

        if (!isEmployee && enrollment.ParticipantId != currentUserId)
            throw new ForbiddenAccessException("A participant can only manage and cancel their own enrollments.");

        enrollment.Cancel(currentUserId, isEmployee, DateTime.UtcNow, enrollment.Workshop.StartAt);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(enrollment, enrollment.Participant, enrollment.Workshop);
    }

    public async Task<BulkEnrollmentResponseDto> BulkEnrollFromExcelAsync(
        Stream fileStream,
        CancellationToken cancellationToken = default)
    {
        if (fileStream is null || fileStream.Length == 0)
            throw new DomainException("The provided Excel file is empty.");

        var rows = await _excelReader.ReadRowsAsync(fileStream, cancellationToken);
        var rejectedRows = new List<BulkEnrollmentRejectedRowDto>();
        var enrolledCount = 0;

        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.ParticipantEmail))
            {
                rejectedRows.Add(new BulkEnrollmentRejectedRowDto(
                    row.RowNumber,
                    row.ParticipantEmail,
                    row.WorkshopCode,
                    "Participant email (ParticipantEmail) is empty."));
                continue;
            }

            if (string.IsNullOrWhiteSpace(row.WorkshopCode))
            {
                rejectedRows.Add(new BulkEnrollmentRejectedRowDto(
                    row.RowNumber,
                    row.ParticipantEmail,
                    row.WorkshopCode,
                    "Workshop code (WorkshopCode) is empty."));
                continue;
            }

            var normalizedEmail = row.ParticipantEmail.Trim().ToLowerInvariant();
            var normalizedCode = row.WorkshopCode.Trim().ToUpperInvariant();

            var participant = await _userRepository.GetParticipantByEmailAsync(normalizedEmail, cancellationToken);
            if (participant is null)
            {
                rejectedRows.Add(new BulkEnrollmentRejectedRowDto(
                    row.RowNumber,
                    row.ParticipantEmail,
                    row.WorkshopCode,
                    $"Participant with email '{normalizedEmail}' does not exist on the platform."));
                continue;
            }

            var workshop = await _workshopRepository.GetByCodeWithDetailsAsync(normalizedCode, cancellationToken);
            if (workshop is null)
            {
                rejectedRows.Add(new BulkEnrollmentRejectedRowDto(
                    row.RowNumber,
                    row.ParticipantEmail,
                    row.WorkshopCode,
                    $"Workshop with code '{normalizedCode}' does not exist on the platform."));
                continue;
            }

            try
            {
                var activeWorkshops = await _workshopRepository.GetActiveWorkshopsByParticipantAsync(participant.Id, cancellationToken);
                var enrollment = workshop.EnrollParticipant(participant, activeWorkshops, DateTime.UtcNow);

                await _enrollmentRepository.AddAsync(enrollment, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                enrolledCount++;
            }
            catch (DomainException ex)
            {
                rejectedRows.Add(new BulkEnrollmentRejectedRowDto(
                    row.RowNumber,
                    row.ParticipantEmail,
                    row.WorkshopCode,
                    ex.Message));
            }
        }

        return new BulkEnrollmentResponseDto(enrolledCount, rejectedRows.Count, rejectedRows);
    }

    public async Task<CertificatePdfDto> GetEnrollmentCertificatePdfAsync(
        Guid enrollmentId,
        Guid currentUserId,
        bool isEmployee,
        CancellationToken cancellationToken = default)
    {
        var enrollment = await _enrollmentRepository.GetByIdWithDetailsAsync(enrollmentId, cancellationToken);
        if (enrollment is null)
            throw new NotFoundException($"Enrollment with ID '{enrollmentId}' was not found.");

        if (!isEmployee && enrollment.ParticipantId != currentUserId)
            throw new ForbiddenAccessException("A participant can only download their own enrollment certificate.");

        var pdfBytes = _pdfGenerator.GenerateEnrollmentCertificate(enrollment);
        var fileName = $"Certificate_{enrollment.Workshop.Code}_{enrollment.Participant.Identification.Number}.pdf";

        return new CertificatePdfDto(pdfBytes, fileName);
    }

    private static EnrollmentResponseDto MapToResponseDto(
        Enrollment enrollment,
        Participant participant,
        Workshop workshop) =>
        new(
            enrollment.Id,
            participant.Id,
            participant.FullName,
            participant.Email.Value,
            workshop.Id,
            workshop.Code,
            workshop.Title,
            workshop.StartAt,
            workshop.EndAt,
            enrollment.Status.ToString(),
            enrollment.CreatedAt,
            enrollment.CancelledAt,
            enrollment.CancelledByUserId);
}
