using BasicCommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BasicCommerce.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);
        builder.HasIndex(t => t.TransactionNumber).IsUnique();
        builder.HasIndex(t => new { t.TenantId, t.StoreId, t.CreatedAt });

        builder.Property(t => t.TransactionNumber).HasMaxLength(50).IsRequired();
        builder.Property(t => t.SubTotal).HasPrecision(18, 4);
        builder.Property(t => t.TaxTotal).HasPrecision(18, 4);
        builder.Property(t => t.DiscountTotal).HasPrecision(18, 4);
        builder.Property(t => t.Total).HasPrecision(18, 4);
        builder.Property(t => t.AmountPaid).HasPrecision(18, 4);
        builder.Property(t => t.ChangeDue).HasPrecision(18, 4);
        builder.Property(t => t.TransactionStatus).HasColumnName("TransactionStatus").HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.VoidReason).HasMaxLength(500);
        builder.Property(t => t.Notes).HasMaxLength(1000);

        builder.HasMany(t => t.LineItems)
            .WithOne()
            .HasForeignKey(l => l.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Payments)
            .WithOne()
            .HasForeignKey(p => p.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(t => t.DomainEvents);
    }
}
