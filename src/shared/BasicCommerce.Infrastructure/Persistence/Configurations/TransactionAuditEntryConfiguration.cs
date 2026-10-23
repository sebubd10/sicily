using BasicCommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BasicCommerce.Infrastructure.Persistence.Configurations;

public class TransactionAuditEntryConfiguration : IEntityTypeConfiguration<TransactionAuditEntry>
{
    public void Configure(EntityTypeBuilder<TransactionAuditEntry> b)
    {
        b.ToTable("TransactionAuditEntries");
        b.HasKey(e => e.Id);
        b.HasIndex(e => new { e.TenantId, e.TransactionId, e.CreatedAt });

        b.Property(e => e.Action).HasConversion<string>().HasMaxLength(40);
        b.Property(e => e.Summary).HasMaxLength(1000).IsRequired();
        b.Property(e => e.Reason).HasMaxLength(500);
    }
}
