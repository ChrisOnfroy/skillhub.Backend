using SkillHub.Application.DTOs.Workshops;

namespace SkillHub.Application.Interfaces.Services;

public interface IWorkshopService
{
    Task<WorkshopResponseDto> CreateWorkshopAsync(CreateWorkshopRequestDto request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkshopResponseDto>> GetAvailableCatalogAsync(CancellationToken cancellationToken = default);
    Task<WorkshopResponseDto> GetWorkshopByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkshopEnrollmentItemDto>> GetWorkshopEnrollmentsAsync(Guid workshopId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TopWorkshopReportItemDto>> GetTopWorkshopsAsync(int top = 10, CancellationToken cancellationToken = default);
}
