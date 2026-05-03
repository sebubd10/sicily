using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BasicCommerce.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.Barcode).IsUnique();
        builder.HasIndex(p => new { p.TenantId, p.Sku }).IsUnique();
        builder.HasIndex(p => p.Plu);

        builder.Property(p => p.Sku).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Barcode).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Plu).HasMaxLength(20);
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.NameBn).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(1000);

        builder.OwnsOne(p => p.Price, money =>
        {
            money.Property(m => m.Amount).HasColumnName("Price").HasPrecision(18, 4);
            money.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3);
        });

        builder.OwnsOne(p => p.CostPrice, money =>
        {
            money.Property(m => m.Amount).HasColumnName("CostPrice").HasPrecision(18, 4);
            money.Property(m => m.Currency).HasColumnName("CostCurrency").HasMaxLength(3);
        });

        builder.Property(p => p.UnitType).HasConversion<string>().HasMaxLength(20);
    }
}
