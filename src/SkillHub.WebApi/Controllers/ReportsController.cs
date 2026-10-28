using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.DTOs.Workshops;
using SkillHub.Application.Interfaces.Services;

namespace SkillHub.WebApi.Controllers;

[Route("api/v1/reports")]
public class ReportsController : ApiControllerBase
{
    private readonly IWorkshopService _workshopService;

    public ReportsController(IWorkshopService workshopService)
    {
        _workshopService = workshopService;
    }

    [HttpGet("top-workshops")]
    [Authorize(Roles = "Employee")]
    public async Task<ActionResult<IReadOnlyList<TopWorkshopReportItemDto>>> GetTopWorkshops([FromQuery] int top = 10, CancellationToken cancellationToken = default)
    {
        var report = await _workshopService.GetTopWorkshopsAsync(top, cancellationToken);
        return Ok(report);
    }
}
