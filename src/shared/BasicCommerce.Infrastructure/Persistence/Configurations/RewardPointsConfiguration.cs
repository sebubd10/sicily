using BasicCommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BasicCommerce.Infrastructure.Persistence.Configurations;

public class RewardPointsSettingsConfiguration : IEntityTypeConfiguration<RewardPointsSettings>
{
    public void Configure(EntityTypeBuilder<RewardPointsSettings> builder)
    {
        builder.HasKey(r => r.Id);
        builder.HasIndex(r => r.TenantId).IsUnique();

        builder.Property(r => r.ExchangeRate).HasPrecision(18, 4);
        builder.Property(r => r.PurchaseSpendPerPoint).HasPrecision(18, 4);
        builder.Property(r => r.MaximumRedeemedRate).HasPrecision(5, 4);
        builder.Property(r => r.MinimumOrderTotalForPoints).HasPrecision(18, 4);
    }
}

public class RewardPointsAccountConfiguration : IEntityTypeConfiguration<RewardPointsAccount>
{
    public void Configure(EntityTypeBuilder<RewardPointsAccount> builder)
    {
        builder.HasKey(r => r.Id);
        builder.HasIndex(r => new { r.TenantId, r.CustomerId, r.StoreId }).IsUnique();

        builder.HasMany(r => r.Entries)
            .WithOne()
            .HasForeignKey(e => e.RewardPointsAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RewardPointsEntryConfiguration : IEntityTypeConfiguration<RewardPointsEntry>
{
    public void Configure(EntityTypeBuilder<RewardPointsEntry> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.RewardPointsAccountId, e.CreatedAt });
        builder.HasIndex(e => e.TransactionId);

        builder.Property(e => e.EntryType).HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.Notes).HasMaxLength(500);
    }
}
