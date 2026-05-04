using BasicCommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BasicCommerce.Infrastructure.Persistence.Configurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.HasKey(w => w.Id);
        builder.HasIndex(w => new { w.TenantId, w.Code }).IsUnique();

        builder.Property(w => w.Name).HasMaxLength(200).IsRequired();
        builder.Property(w => w.Code).HasMaxLength(50).IsRequired();
        builder.Property(w => w.Phone).HasMaxLength(50);
        builder.Property(w => w.Email).HasMaxLength(200);

        builder.OwnsOne(w => w.Address, addr =>
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

public class WarehouseStockLevelConfiguration : IEntityTypeConfiguration<WarehouseStockLevel>
{
    public void Configure(EntityTypeBuilder<WarehouseStockLevel> builder)
    {
        builder.HasKey(w => w.Id);
        builder.HasIndex(w => new { w.TenantId, w.WarehouseId, w.ProductId }).IsUnique();

        builder.Property(w => w.Quantity).HasPrecision(18, 4);
        builder.Property(w => w.ReservedQuantity).HasPrecision(18, 4);
        builder.Property(w => w.LowStockThreshold).HasPrecision(18, 4);

        builder.HasOne(w => w.Warehouse)
            .WithMany()
            .HasForeignKey(w => w.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.Product)
            .WithMany()
            .HasForeignKey(w => w.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class WarehouseMovementConfiguration : IEntityTypeConfiguration<WarehouseMovement>
{
    public void Configure(EntityTypeBuilder<WarehouseMovement> builder)
    {
        builder.HasKey(w => w.Id);
        builder.HasIndex(w => new { w.TenantId, w.WarehouseId, w.CreatedAt });
        builder.HasIndex(w => new { w.TenantId, w.WarehouseId, w.ProductId });

        builder.Property(w => w.MovementType).HasConversion<string>().HasMaxLength(30);
        builder.Property(w => w.Quantity).HasPrecision(18, 4);
        builder.Property(w => w.QuantityBefore).HasPrecision(18, 4);
        builder.Property(w => w.QuantityAfter).HasPrecision(18, 4);
        builder.Property(w => w.Reference).HasMaxLength(100);
        builder.Property(w => w.Notes).HasMaxLength(500);

        builder.HasOne(w => w.Warehouse)
            .WithMany()
            .HasForeignKey(w => w.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.Product)
            .WithMany()
            .HasForeignKey(w => w.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
