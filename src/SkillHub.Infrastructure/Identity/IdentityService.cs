using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SkillHub.Application.DTOs.Auth;
using SkillHub.Application.Interfaces.Auth;
using SkillHub.Domain.Commons;
using SkillHub.Domain.Exceptions;
using SkillHub.Infrastructure.Context;
using SkillHub.Infrastructure.Settings;

namespace SkillHub.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ApplicationDbContext _context;
    private readonly JwtSettings _jwtSettings;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        ApplicationDbContext context,
        IOptions<JwtSettings> jwtOptions)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task CreateCredentialsAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        var appUser = new ApplicationUser(user.Id, user.Email.Value);

        var result = await _userManager.CreateAsync(appUser, password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new DomainException($"Error creating credentials: {errors}");
        }

        var roleName = user.Role.ToString();
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        }

        await _userManager.AddToRoleAsync(appUser, roleName);
    }

    public async Task<AuthResponseDto> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var appUser = await _userManager.FindByEmailAsync(normalizedEmail);
        if (appUser is null)
            throw new DomainException("Invalid credentials or inactive user.");

        var passwordValid = await _userManager.CheckPasswordAsync(appUser, password);
        if (!passwordValid)
            throw new DomainException("Invalid credentials or inactive user.");

        var domainUser = await _context.DomainUsers.FirstOrDefaultAsync(u => u.Id == appUser.Id, cancellationToken);
        if (domainUser is null || !domainUser.IsActive)
            throw new DomainException("User is inactive or does not exist.");

        return await GenerateTokenForUserAsync(domainUser, cancellationToken);
    }

    public async Task<AuthResponseDto> GenerateTokenForUserAsync(User user, CancellationToken cancellationToken = default)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        var roles = appUser is not null
            ? await _userManager.GetRolesAsync(appUser)
            : new List<string> { user.Role.ToString() };

        var primaryRole = roles.FirstOrDefault() ?? user.Role.ToString();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email.Value),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, primaryRole),
            new("role", primaryRole)
        };

        foreach (var role in roles)
        {
            if (!claims.Any(c => c.Type == ClaimTypes.Role && c.Value == role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return new AuthResponseDto(
            tokenString,
            user.Id,
            user.FullName,
            user.Email.Value,
            primaryRole,
            expiresAt);
    }

    public async Task<string?> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var appUser = await _userManager.FindByEmailAsync(normalizedEmail);
        if (appUser is null)
            return null;

        var domainUser = await _context.DomainUsers.FirstOrDefaultAsync(u => u.Id == appUser.Id, cancellationToken);
        if (domainUser is null || !domainUser.IsActive)
            return null;

        return await _userManager.GeneratePasswordResetTokenAsync(appUser);
    }

    public async Task ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var appUser = await _userManager.FindByEmailAsync(normalizedEmail);
        if (appUser is null)
            throw new DomainException("No user found with the provided email address.");

        var domainUser = await _context.DomainUsers.FirstOrDefaultAsync(u => u.Id == appUser.Id, cancellationToken);
        if (domainUser is null || !domainUser.IsActive)
            throw new DomainException("The user account is inactive or does not exist.");

        var result = await _userManager.ResetPasswordAsync(appUser, token, newPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new DomainException($"Failed to reset password: {errors}");
        }
    }

    public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var appUser = await _userManager.FindByIdAsync(userId.ToString());
        if (appUser is null)
            throw new NotFoundException($"User with ID '{userId}' was not found.");

        var domainUser = await _context.DomainUsers.FirstOrDefaultAsync(u => u.Id == appUser.Id, cancellationToken);
        if (domainUser is null || !domainUser.IsActive)
            throw new DomainException("The user account is inactive or does not exist.");

        var result = await _userManager.ChangePasswordAsync(appUser, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new DomainException($"Failed to change password: {errors}");
        }
    }
}
