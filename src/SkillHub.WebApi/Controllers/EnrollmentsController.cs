using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.DTOs.Enrollments;
using SkillHub.Application.Interfaces.Services;

namespace SkillHub.WebApi.Controllers;

[Route("api/v1/enrollments")]
public class EnrollmentsController : ApiControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpPost]
    [Authorize(Roles = "Participant")]
    public async Task<ActionResult<EnrollmentResponseDto>> Enroll([FromBody] CreateEnrollmentRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _enrollmentService.EnrollAsync(CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("mine")]
    [Authorize(Roles = "Participant")]
    public async Task<ActionResult<IReadOnlyList<EnrollmentResponseDto>>> GetMine(CancellationToken cancellationToken)
    {
        var result = await _enrollmentService.GetMyEnrollmentsAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/cancel")]
    [Authorize]
    public async Task<ActionResult<EnrollmentResponseDto>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _enrollmentService.CancelEnrollmentAsync(id, CurrentUserId, IsCurrentUserEmployee, cancellationToken);
        return Ok(result);
    }

    [HttpPost("import")]
    [Authorize(Roles = "Employee")]
    public async Task<ActionResult<BulkEnrollmentResponseDto>> Import(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "A valid Excel file must be provided." });

        using var stream = file.OpenReadStream();
        var result = await _enrollmentService.BulkEnrollFromExcelAsync(stream, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/certificate")]
    [Authorize]
    public async Task<IActionResult> GetCertificate(Guid id, CancellationToken cancellationToken)
    {
        var certificate = await _enrollmentService.GetEnrollmentCertificatePdfAsync(id, CurrentUserId, IsCurrentUserEmployee, cancellationToken);
        return File(certificate.Content, "application/pdf", certificate.FileName);
    }
}
