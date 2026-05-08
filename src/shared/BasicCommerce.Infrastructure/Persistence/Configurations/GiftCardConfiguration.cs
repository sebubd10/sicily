using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BasicCommerce.Infrastructure.Persistence.Configurations;

public class GiftCardConfiguration : IEntityTypeConfiguration<GiftCard>
{
    public void Configure(EntityTypeBuilder<GiftCard> b)
    {
        b.ToTable("GiftCards");
        b.HasKey(x => x.Id);

        b.Property(x => x.Code).HasMaxLength(50).IsRequired();
        b.Property(x => x.InitialBalance).HasPrecision(18, 4);
        b.Property(x => x.Balance).HasPrecision(18, 4);
        b.Property(x => x.Notes).HasMaxLength(500);
        b.Property(x => x.CardStatus)
            .HasConversion<string>().HasMaxLength(20)
            .HasDefaultValue(GiftCardStatus.Inactive);

        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.StoreId, x.CardStatus });

        b.HasMany(x => x.Transactions)
            .WithOne()
            .HasForeignKey(x => x.GiftCardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class GiftCardTransactionConfiguration : IEntityTypeConfiguration<GiftCardTransaction>
{
    public void Configure(EntityTypeBuilder<GiftCardTransaction> b)
    {
        b.ToTable("GiftCardTransactions");
        b.HasKey(x => x.Id);

        b.Property(x => x.Amount).HasPrecision(18, 4);
        b.Property(x => x.BalanceAfter).HasPrecision(18, 4);
        b.Property(x => x.Notes).HasMaxLength(500);
        b.Property(x => x.TransactionType)
            .HasConversion<string>().HasMaxLength(20);

        b.HasIndex(x => x.GiftCardId);
    }
}
