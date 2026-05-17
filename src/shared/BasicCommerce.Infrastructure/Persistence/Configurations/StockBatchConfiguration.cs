using BasicCommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BasicCommerce.Infrastructure.Persistence.Configurations;

public class StockBatchConfiguration : IEntityTypeConfiguration<StockBatch>
{
    public void Configure(EntityTypeBuilder<StockBatch> builder)
    {
        builder.ToTable("StockBatches");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.LotNumber).HasMaxLength(100);
        builder.Property(b => b.ReceivedQuantity).HasPrecision(18, 4);
        builder.Property(b => b.RemainingQuantity).HasPrecision(18, 4);
        builder.Property(b => b.UnitCost).HasPrecision(18, 4);
        builder.Property(b => b.IsExpired).HasDefaultValue(false);

        builder.HasOne(b => b.Product)
            .WithMany()
            .HasForeignKey(b => b.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Store)
            .WithMany()
            .HasForeignKey(b => b.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => new { b.TenantId, b.StoreId, b.ProductId, b.IsExpired });
        builder.HasIndex(b => b.ExpiryDate);
    }
}
