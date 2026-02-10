using Amazon;
using Amazon.SimpleNotificationService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RegistrationEventService.Domain.Abstractions;
using RegistrationEventService.Infrastructure.Messaging;
using RegistrationEventService.Infrastructure.Persistence;
using RegistrationEventService.Infrastructure.Persistence.Repositories;

namespace RegistrationEventService.Infrastructure;

/// <summary>
/// Dependency injection extensions for the Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers infrastructure-layer services (EF Core, AWS SNS, etc.) into the DI container.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EF Core + SQL Server
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            }));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // AWS SNS
        // services.Configure<SnsOptions>(configuration.GetSection(SnsOptions.SectionName));

        // var snsOptions = configuration.GetSection(SnsOptions.SectionName).Get<SnsOptions>()
        //     ?? new SnsOptions();

        // services.AddSingleton<IAmazonSimpleNotificationService>(_ =>
        //     new AmazonSimpleNotificationServiceClient(RegionEndpoint.GetBySystemName(snsOptions.Region)));

        // services.AddScoped<IEventPublisher, SnsEventPublisher>();

        return services;
    }
}
