using Microsoft.EntityFrameworkCore;
using RegistrationEventService.Domain.Abstractions;
using RegistrationEventService.Domain.Entities;

namespace RegistrationEventService.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for the application.
/// Acts as the Unit of Work and provides DbSets for domain entities.
/// </summary>
public sealed class ApplicationDbContext : DbContext, IUnitOfWork
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
