using SkillHub.Application.DTOs.Enrollments;

namespace SkillHub.Application.Interfaces.Services;

public interface IExcelEnrollmentReader
{
    Task<IReadOnlyList<BulkEnrollmentRowDto>> ReadRowsAsync(Stream fileStream, CancellationToken cancellationToken = default);
}
