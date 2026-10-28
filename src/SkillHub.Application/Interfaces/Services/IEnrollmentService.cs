using SkillHub.Application.DTOs.Enrollments;

namespace SkillHub.Application.Interfaces.Services;

public interface IEnrollmentService
{
    Task<EnrollmentResponseDto> EnrollAsync(Guid participantId, CreateEnrollmentRequestDto request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EnrollmentResponseDto>> GetMyEnrollmentsAsync(Guid participantId, CancellationToken cancellationToken = default);
    Task<EnrollmentResponseDto> CancelEnrollmentAsync(Guid enrollmentId, Guid currentUserId, bool isEmployee, CancellationToken cancellationToken = default);
    Task<BulkEnrollmentResponseDto> BulkEnrollFromExcelAsync(Stream fileStream, CancellationToken cancellationToken = default);
    Task<CertificatePdfDto> GetEnrollmentCertificatePdfAsync(Guid enrollmentId, Guid currentUserId, bool isEmployee, CancellationToken cancellationToken = default);
}
