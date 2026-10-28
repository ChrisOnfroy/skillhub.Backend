using System.Security.Cryptography;
using SkillHub.Application.DTOs.Auth;
using SkillHub.Application.Interfaces.Auth;
using SkillHub.Application.Interfaces.Repositories;
using SkillHub.Application.Interfaces.Services;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Exceptions;
using SkillHub.Domain.ValueObjects;

namespace SkillHub.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityService _identityService;
    private readonly IEmailService _emailService;
    private readonly IPasswordResetCodeStore _codeStore;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        IUserRepository userRepository,
        IIdentityService identityService,
        IEmailService emailService,
        IPasswordResetCodeStore codeStore,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _identityService = identityService;
        _emailService = emailService;
        _codeStore = codeStore;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> RegisterParticipantAsync(
        RegisterParticipantRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            throw new DomainException("Password is required and must be at least 6 characters long.");

        var email = new Email(request.Email);
        var identification = new IdentificationDocument(request.IdentificationType, request.IdentificationNumber);

        if (await _userRepository.ExistsByEmailAsync(email.Value, cancellationToken))
            throw new DomainException($"A user with email '{email.Value}' already exists.");

        if (await _userRepository.ExistsByIdentificationAsync(identification.Type, identification.Number, cancellationToken))
            throw new DomainException($"A user with identification document '{identification}' already exists.");

        PhoneNumber? phoneNumber = null;
        if (!string.IsNullOrWhiteSpace(request.PhoneCountryCode) && !string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            phoneNumber = new PhoneNumber(request.PhoneCountryCode, request.PhoneNumber);
        }

        Address? address = null;
        if (!string.IsNullOrWhiteSpace(request.Street) &&
            !string.IsNullOrWhiteSpace(request.City) &&
            !string.IsNullOrWhiteSpace(request.Country))
        {
            address = new Address(
                request.Street,
                request.City,
                request.State ?? string.Empty,
                request.PostalCode ?? string.Empty,
                request.Country);
        }

        var participant = new Participant(
            request.FirstName,
            request.LastName,
            email,
            identification,
            phoneNumber,
            address);

        await _userRepository.AddAsync(participant, cancellationToken);
        await _identityService.CreateCredentialsAsync(participant, request.Password, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await _identityService.GenerateTokenForUserAsync(participant, cancellationToken);
    }

    public async Task<AuthResponseDto> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            throw new DomainException("Email and password must be provided.");

        return await _identityService.AuthenticateAsync(request.Email.Trim().ToLowerInvariant(), request.Password, cancellationToken);
    }

    public async Task<ForgotPasswordResponseDto> ForgotPasswordAsync(
        ForgotPasswordRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.IdentificationNumber))
            throw new DomainException("El número de documento de identificación es obligatorio.");

        var user = await _userRepository.GetByIdentificationNumberAsync(request.IdentificationNumber, cancellationToken);

        if (user != null && user.IsActive)
        {
            var identityToken = await _identityService.GeneratePasswordResetTokenAsync(user.Email.Value, cancellationToken);
            if (!string.IsNullOrEmpty(identityToken))
            {
                var verificationCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
                var ttl = TimeSpan.FromMinutes(15);

                var codeData = new PasswordResetCodeData
                {
                    IdentificationNumber = user.Identification.Number,
                    Email = user.Email.Value,
                    Code = verificationCode,
                    IdentityToken = identityToken,
                    ExpiresAtUtc = DateTime.UtcNow.Add(ttl),
                    FailedAttempts = 0
                };

                _codeStore.SaveCode(user.Identification.Number, codeData, ttl);

                await _emailService.SendPasswordResetCodeEmailAsync(
                    user.Email.Value,
                    user.FullName,
                    verificationCode,
                    cancellationToken);

                return new ForgotPasswordResponseDto(
                    "Se ha enviado un código de verificación de 6 dígitos al correo electrónico registrado.",
                    MaskEmail(user.Email.Value));
            }
        }

        return new ForgotPasswordResponseDto(
            "Si el número de documento se encuentra registrado, se enviará un código de verificación al correo asociado.");
    }

    public Task<VerifyResetCodeResponseDto> VerifyResetCodeAsync(
        VerifyResetCodeRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.IdentificationNumber))
            throw new DomainException("El número de documento de identificación es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Code))
            throw new DomainException("El código de verificación es obligatorio.");

        var cached = _codeStore.GetCode(request.IdentificationNumber.Trim());
        if (cached == null || cached.ExpiresAtUtc < DateTime.UtcNow)
        {
            throw new DomainException("El código de verificación es inválido o ha expirado. Por favor solicita uno nuevo.");
        }

        if (cached.FailedAttempts >= 5)
        {
            _codeStore.RemoveCode(request.IdentificationNumber.Trim());
            throw new DomainException("Has superado el número máximo de intentos fallidos. Por favor solicita un nuevo código.");
        }

        if (!string.Equals(cached.Code.Trim(), request.Code.Trim(), StringComparison.Ordinal))
        {
            cached.FailedAttempts++;
            throw new DomainException("El código de verificación es incorrecto.");
        }

        return Task.FromResult(new VerifyResetCodeResponseDto(
            IsValid: true,
            Message: "Código verificado exitosamente. Ahora puedes ingresar tu nueva contraseña.",
            Email: cached.Email,
            ResetToken: cached.IdentityToken));
    }

    public async Task<ResetPasswordResponseDto> ResetPasswordAsync(
        ResetPasswordRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            throw new DomainException("La nueva contraseña es obligatoria y debe tener al menos 6 caracteres.");

        if (request.NewPassword != request.ConfirmPassword)
            throw new DomainException("La nueva contraseña y su confirmación no coinciden.");

        string email;
        string token;

        if (!string.IsNullOrWhiteSpace(request.Email) && !string.IsNullOrWhiteSpace(request.Token))
        {
            email = request.Email.Trim();
            token = request.Token.Trim();
        }
        else if (!string.IsNullOrWhiteSpace(request.IdentificationNumber) && !string.IsNullOrWhiteSpace(request.Code))
        {
            var cached = _codeStore.GetCode(request.IdentificationNumber.Trim());
            if (cached == null || cached.ExpiresAtUtc < DateTime.UtcNow)
                throw new DomainException("El código de verificación es inválido o ha expirado. Por favor solicita uno nuevo.");

            if (!string.Equals(cached.Code.Trim(), request.Code.Trim(), StringComparison.Ordinal))
                throw new DomainException("El código de verificación es incorrecto.");

            email = cached.Email;
            token = cached.IdentityToken;
        }
        else
        {
            throw new DomainException("Debes proporcionar el token de verificación o el código junto al documento de identidad.");
        }

        var validEmail = new Email(email);
        await _identityService.ResetPasswordAsync(validEmail.Value, token, request.NewPassword, cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.IdentificationNumber))
        {
            _codeStore.RemoveCode(request.IdentificationNumber.Trim());
        }

        return new ResetPasswordResponseDto("Tu contraseña ha sido restablecida exitosamente.");
    }

    public async Task<ResetPasswordResponseDto> ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Invalid user ID.");

        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            throw new DomainException("Current password is required.");

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            throw new DomainException("New password is required and must be at least 6 characters long.");

        if (request.NewPassword != request.ConfirmPassword)
            throw new DomainException("New password and confirmation password do not match.");

        await _identityService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword, cancellationToken);

        return new ResetPasswordResponseDto("Password has been changed successfully.");
    }

    private static string MaskEmail(string email)
    {
        var parts = email.Split('@');
        if (parts.Length != 2) return "***";
        var name = parts[0];
        var domain = parts[1];
        if (name.Length <= 2)
            return $"{name[0]}*@{domain}";
        return $"{name[0]}{new string('*', Math.Min(name.Length - 2, 6))}{name[^1]}@{domain}";
    }
}
