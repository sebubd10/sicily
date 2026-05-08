using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BasicCommerce.Infrastructure.Persistence.Configurations;

public class TillSessionConfiguration : IEntityTypeConfiguration<TillSession>
{
    public void Configure(EntityTypeBuilder<TillSession> b)
    {
        b.ToTable("TillSessions");
        b.HasKey(x => x.Id);

        b.Property(x => x.OpeningFloat).HasPrecision(18, 4);
        b.Property(x => x.ClosingBalance).HasPrecision(18, 4);
        b.Property(x => x.ClosingVariance).HasPrecision(18, 4);
        b.Property(x => x.ExpectedClosingBalance).HasPrecision(18, 4);
        b.Property(x => x.Notes).HasMaxLength(500);
        b.Property(x => x.SessionStatus)
            .HasConversion<string>().HasMaxLength(20)
            .HasDefaultValue(TillSessionStatus.Open);

        b.HasIndex(x => new { x.TenantId, x.TerminalId, x.SessionStatus });
        b.HasIndex(x => new { x.TenantId, x.StoreId });

        b.HasMany(x => x.PettyTransactions)
            .WithOne()
            .HasForeignKey(x => x.TillSessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PettyTransactionConfiguration : IEntityTypeConfiguration<PettyTransaction>
{
    public void Configure(EntityTypeBuilder<PettyTransaction> b)
    {
        b.ToTable("PettyTransactions");
        b.HasKey(x => x.Id);

        b.Property(x => x.Amount).HasPrecision(18, 4);
        b.Property(x => x.Reason).HasMaxLength(200).IsRequired();
        b.Property(x => x.Type)
            .HasConversion<string>().HasMaxLength(20);

        b.HasIndex(x => x.TillSessionId);
    }
}
