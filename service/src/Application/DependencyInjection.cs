using Microsoft.Extensions.DependencyInjection;
using RegistrationEventService.Application.Abstractions;
using RegistrationEventService.Application.Services;

namespace RegistrationEventService.Application;

/// <summary>
/// Dependency injection extensions for the Application layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers application-layer services into the DI container.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProductService, ProductService>();

        return services;
    }
}
