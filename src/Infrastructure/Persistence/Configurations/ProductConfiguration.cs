using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistrationEventService.Domain.Entities;

namespace RegistrationEventService.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="Product"/> entity.
/// Maps domain entity properties to SQL Server table schema.
/// </summary>
public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", "catalog");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Sku)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(p => p.Supplier)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(p => p.Sku)
            .IsUnique();

        builder.Property(p => p.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.CreatedAt)
            .IsRequired();
    }
}
