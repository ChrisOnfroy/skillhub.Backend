using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.DTOs.Workshops;
using SkillHub.Application.Interfaces.Services;

namespace SkillHub.WebApi.Controllers;

[Route("api/v1/workshops")]
public class WorkshopsController : ApiControllerBase
{
    private readonly IWorkshopService _workshopService;

    public WorkshopsController(IWorkshopService workshopService)
    {
        _workshopService = workshopService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<WorkshopResponseDto>>> GetCatalog(CancellationToken cancellationToken)
    {
        var workshops = await _workshopService.GetAvailableCatalogAsync(cancellationToken);
        return Ok(workshops);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<WorkshopResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var workshop = await _workshopService.GetWorkshopByIdAsync(id, cancellationToken);
        return Ok(workshop);
    }

    [HttpPost]
    [Authorize(Roles = "Employee")]
    public async Task<ActionResult<WorkshopResponseDto>> Create([FromBody] CreateWorkshopRequestDto request, CancellationToken cancellationToken)
    {
        var created = await _workshopService.CreateWorkshopAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("{id:guid}/enrollments")]
    [Authorize(Roles = "Employee")]
    public async Task<ActionResult<IReadOnlyList<WorkshopEnrollmentItemDto>>> GetEnrollments(Guid id, CancellationToken cancellationToken)
    {
        var enrollments = await _workshopService.GetWorkshopEnrollmentsAsync(id, cancellationToken);
        return Ok(enrollments);
    }
}
