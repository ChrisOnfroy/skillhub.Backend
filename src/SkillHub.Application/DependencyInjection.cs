using Microsoft.Extensions.DependencyInjection;
using SkillHub.Application.Interfaces.Auth;
using SkillHub.Application.Interfaces.Services;
using SkillHub.Application.Services;

namespace SkillHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IWorkshopService, WorkshopService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();

        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
        => services.AddApplicationServices();
}
