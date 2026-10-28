using SkillHub.Domain.Enums;

namespace SkillHub.Application.DTOs.Auth;

public record RegisterParticipantRequestDto(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    IdentificationType IdentificationType,
    string IdentificationNumber,
    string? PhoneCountryCode = null,
    string? PhoneNumber = null,
    string? Street = null,
    string? City = null,
    string? State = null,
    string? PostalCode = null,
    string? Country = null);
