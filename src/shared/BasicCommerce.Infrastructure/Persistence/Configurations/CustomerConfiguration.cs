using BasicCommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BasicCommerce.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => new { c.TenantId, c.Code }).IsUnique();
        builder.HasIndex(c => new { c.TenantId, c.Email });

        builder.Property(c => c.Code).HasMaxLength(50).IsRequired();
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Email).HasMaxLength(200);
        builder.Property(c => c.Phone).HasMaxLength(50);
        builder.Property(c => c.CreditLimit).HasPrecision(18, 4);
        builder.Property(c => c.CurrentBalance).HasPrecision(18, 4);

        builder.OwnsOne(c => c.Address, addr =>
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
