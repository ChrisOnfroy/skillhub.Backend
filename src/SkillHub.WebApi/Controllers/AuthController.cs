using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.DTOs.Auth;
using SkillHub.Application.Interfaces.Auth;

namespace SkillHub.WebApi.Controllers;

[Route("api/v1/auth")]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterParticipantRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _authService.RegisterParticipantAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ForgotPasswordResponseDto>> ForgotPassword([FromBody] ForgotPasswordRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _authService.ForgotPasswordAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("verify-reset-code")]
    [AllowAnonymous]
    public async Task<ActionResult<VerifyResetCodeResponseDto>> VerifyResetCode([FromBody] VerifyResetCodeRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _authService.VerifyResetCodeAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ResetPasswordResponseDto>> ResetPassword([FromBody] ResetPasswordRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _authService.ResetPasswordAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ResetPasswordResponseDto>> ChangePassword([FromBody] ChangePasswordRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _authService.ChangePasswordAsync(CurrentUserId, request, cancellationToken);
        return Ok(response);
    }
}
