using BasicCommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BasicCommerce.Infrastructure.Persistence.Configurations;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => new { p.TenantId, p.OrderNumber }).IsUnique();
        builder.HasIndex(p => new { p.TenantId, p.PurchaseOrderStatus });
        builder.HasIndex(p => new { p.TenantId, p.SupplierId });

        builder.Property(p => p.OrderNumber).HasMaxLength(50).IsRequired();
        builder.Property(p => p.PurchaseOrderStatus).HasConversion<string>().HasMaxLength(30);
        builder.Property(p => p.Currency).HasMaxLength(3);
        builder.Property(p => p.Notes).HasMaxLength(1000);

        builder.HasOne(p => p.Supplier)
            .WithMany()
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Warehouse)
            .WithMany()
            .HasForeignKey(p => p.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Items)
            .WithOne()
            .HasForeignKey(i => i.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.HasIndex(i => new { i.PurchaseOrderId, i.ProductId }).IsUnique();

        builder.Property(i => i.OrderedQuantity).HasPrecision(18, 4);
        builder.Property(i => i.ReceivedQuantity).HasPrecision(18, 4);
        builder.Property(i => i.UnitCost).HasPrecision(18, 4);

        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
