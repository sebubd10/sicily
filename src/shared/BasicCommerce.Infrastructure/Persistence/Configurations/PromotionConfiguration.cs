using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BasicCommerce.Infrastructure.Persistence.Configurations;

public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> b)
    {
        b.ToTable("Promotions");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Description).HasMaxLength(1000);
        b.Property(x => x.DiscountPercentage).HasPrecision(18, 4);
        b.Property(x => x.DiscountAmount).HasPrecision(18, 4);
        b.Property(x => x.MinimumCartValue).HasPrecision(18, 4);
        b.Property(x => x.CouponCode).HasMaxLength(50);
        b.Property(x => x.Type)
            .HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.PromotionStatus)
            .HasConversion<string>().HasMaxLength(20)
            .HasDefaultValue(PromotionStatus.Draft);

        b.HasIndex(x => new { x.TenantId, x.PromotionStatus });
        b.HasIndex(x => new { x.TenantId, x.CouponCode })
            .IsUnique()
            .HasFilter("[CouponCode] IS NOT NULL");
    }
}
