using BasicCommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BasicCommerce.Infrastructure.Persistence.Configurations;

public class SupplierReturnConfiguration : IEntityTypeConfiguration<SupplierReturn>
{
    public void Configure(EntityTypeBuilder<SupplierReturn> builder)
    {
        builder.ToTable("SupplierReturns");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ReturnNumber).HasMaxLength(50).IsRequired();
        builder.Property(r => r.Notes).HasMaxLength(1000);
        builder.Property(r => r.CreditNoteReference).HasMaxLength(100);
        builder.Property(r => r.ExpectedCreditAmount).HasPrecision(18, 4);
        builder.Property(r => r.ActualCreditAmount).HasPrecision(18, 4);
        builder.Property(r => r.ReturnStatus).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(r => r.ReturnNumber).IsUnique();
        builder.HasIndex(r => new { r.TenantId, r.SupplierId, r.ReturnStatus });

        builder.HasOne(r => r.Supplier)
            .WithMany()
            .HasForeignKey(r => r.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Store)
            .WithMany()
            .HasForeignKey(r => r.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.PurchaseOrder)
            .WithMany()
            .HasForeignKey(r => r.PurchaseOrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(r => r.Items)
            .WithOne()
            .HasForeignKey(i => i.SupplierReturnId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SupplierReturnItemConfiguration : IEntityTypeConfiguration<SupplierReturnItem>
{
    public void Configure(EntityTypeBuilder<SupplierReturnItem> builder)
    {
        builder.ToTable("SupplierReturnItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Quantity).HasPrecision(18, 4);
        builder.Property(i => i.UnitCost).HasPrecision(18, 4);
        builder.Property(i => i.Reason).HasConversion<string>().HasMaxLength(30);
        builder.Property(i => i.Notes).HasMaxLength(500);

        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.SupplierReturnId);
    }
}
