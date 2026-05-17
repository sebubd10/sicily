using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BasicCommerce.Infrastructure.Persistence.Configurations;

public class LineItemConfiguration : IEntityTypeConfiguration<LineItem>
{
    public void Configure(EntityTypeBuilder<LineItem> builder)
    {
        builder.ToTable("LineItems");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(l => l.ProductSku).HasMaxLength(50).IsRequired();
        builder.Property(l => l.Quantity).HasPrecision(18, 4);
        builder.Property(l => l.UnitPrice).HasPrecision(18, 4);
        builder.Property(l => l.TaxRate).HasPrecision(6, 4);
        builder.Property(l => l.TaxAmount).HasPrecision(18, 4);
        builder.Property(l => l.DiscountAmount).HasPrecision(18, 4);
        builder.Property(l => l.LineTotal).HasPrecision(18, 4);

        builder.Property(l => l.ReturnReason)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(l => l.DamageDisposition)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(l => l.AppliedPromotionName).HasMaxLength(200);
    }
}
