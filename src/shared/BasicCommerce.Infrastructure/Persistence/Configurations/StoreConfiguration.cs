using BasicCommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BasicCommerce.Infrastructure.Persistence.Configurations;

public class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => new { s.TenantId, s.Code }).IsUnique();

        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Code).HasMaxLength(50).IsRequired();
        builder.Property(s => s.Phone).HasMaxLength(50);
        builder.Property(s => s.Email).HasMaxLength(200);

        builder.OwnsOne(s => s.Address, addr =>
        {
            addr.Property(a => a.Line1).HasColumnName("AddressLine1").HasMaxLength(200);
            addr.Property(a => a.Line2).HasColumnName("AddressLine2").HasMaxLength(200);
            addr.Property(a => a.City).HasColumnName("AddressCity").HasMaxLength(100);
            addr.Property(a => a.District).HasColumnName("AddressDistrict").HasMaxLength(100);
            addr.Property(a => a.PostalCode).HasColumnName("AddressPostalCode").HasMaxLength(20);
            addr.Property(a => a.Country).HasColumnName("AddressCountry").HasMaxLength(100);
        });
    }
}
