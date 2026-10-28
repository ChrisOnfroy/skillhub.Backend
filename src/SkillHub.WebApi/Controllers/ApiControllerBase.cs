using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace SkillHub.WebApi.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected Guid CurrentUserId
    {
        get
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(idClaim, out var id) ? id : Guid.Empty;
        }
    }

    protected string CurrentUserRole =>
        User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

    protected bool IsCurrentUserEmployee =>
        User.IsInRole("Employee") || CurrentUserRole.Equals("Employee", StringComparison.OrdinalIgnoreCase);
}
