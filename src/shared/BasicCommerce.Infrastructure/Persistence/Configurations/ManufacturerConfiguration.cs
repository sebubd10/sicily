using BasicCommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BasicCommerce.Infrastructure.Persistence.Configurations;

public class ManufacturerConfiguration : IEntityTypeConfiguration<Manufacturer>
{
    public void Configure(EntityTypeBuilder<Manufacturer> builder)
    {
        builder.HasKey(m => m.Id);
        builder.HasIndex(m => new { m.TenantId, m.Name });
        builder.HasIndex(m => new { m.TenantId, m.Code });

        builder.Property(m => m.Name).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Code).HasMaxLength(50);
        builder.Property(m => m.Country).HasMaxLength(100);
        builder.Property(m => m.Website).HasMaxLength(500);
        builder.Property(m => m.ContactEmail).HasMaxLength(200);
        builder.Property(m => m.Notes).HasMaxLength(1000);
    }
}
