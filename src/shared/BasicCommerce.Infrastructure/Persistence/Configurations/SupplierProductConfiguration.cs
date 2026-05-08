using BasicCommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BasicCommerce.Infrastructure.Persistence.Configurations;

public class SupplierProductConfiguration : IEntityTypeConfiguration<SupplierProduct>
{
    public void Configure(EntityTypeBuilder<SupplierProduct> b)
    {
        b.ToTable("SupplierProducts");
        b.HasKey(x => x.Id);

        b.Property(x => x.SupplierSku).HasMaxLength(100);
        b.Property(x => x.UnitCost).HasPrecision(18, 4);
        b.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("BDT");
        b.Property(x => x.Notes).HasMaxLength(500);

        b.HasOne(x => x.Supplier).WithMany()
            .HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Product).WithMany()
            .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.TenantId, x.SupplierId, x.ProductId }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.ProductId });
    }
}
