using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BasicCommerce.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> b)
    {
        b.HasKey(t => t.Id);
        b.HasIndex(t => t.Slug).IsUnique();

        b.Property(t => t.Name).HasMaxLength(200).IsRequired();
        b.Property(t => t.Slug).HasMaxLength(100).IsRequired();
        b.Property(t => t.ContactEmail).HasMaxLength(256).IsRequired();
        b.Property(t => t.ContactPhone).HasMaxLength(20);
        b.Property(t => t.BusinessIdentificationNumber).HasMaxLength(50);
        b.Property(t => t.VatRegistrationNumber).HasMaxLength(50);
        b.Property(t => t.CurrencyCode).HasMaxLength(3).HasDefaultValue("BDT");
        b.Property(t => t.DefaultLanguage).HasMaxLength(10).HasDefaultValue("en");
    }
}

public class VatRateConfiguration : IEntityTypeConfiguration<VatRate>
{
    public void Configure(EntityTypeBuilder<VatRate> b)
    {
        b.HasKey(v => v.Id);
        b.HasIndex(v => new { v.TenantId, v.Code }).IsUnique();

        b.Property(v => v.Name).HasMaxLength(100).IsRequired();
        b.Property(v => v.Code).HasMaxLength(20).IsRequired();
        b.Property(v => v.Rate).HasPrecision(6, 4);
        b.Property(v => v.IsDefault).HasDefaultValue(false);
    }
}

public class TerminalConfiguration : IEntityTypeConfiguration<Terminal>
{
    public void Configure(EntityTypeBuilder<Terminal> b)
    {
        b.HasKey(t => t.Id);
        b.HasIndex(t => new { t.TenantId, t.StoreId, t.Code }).IsUnique();

        b.Property(t => t.Name).HasMaxLength(100).IsRequired();
        b.Property(t => t.Code).HasMaxLength(20).IsRequired();
        b.Property(t => t.OpeningFloat).HasPrecision(18, 4);
        b.Property(t => t.Type).HasConversion<string>().HasMaxLength(20);
        b.Property(t => t.TerminalStatus).HasConversion<string>().HasMaxLength(20);
    }
}

public class StockLevelConfiguration : IEntityTypeConfiguration<StockLevel>
{
    public void Configure(EntityTypeBuilder<StockLevel> b)
    {
        b.HasKey(s => s.Id);
        b.HasIndex(s => new { s.TenantId, s.StoreId, s.ProductId }).IsUnique();

        b.Property(s => s.Quantity).HasPrecision(18, 4);
        b.Property(s => s.ReservedQuantity).HasPrecision(18, 4);
        b.Property(s => s.LowStockThreshold).HasPrecision(18, 4).HasDefaultValue(10m);

        b.Ignore(s => s.AvailableQuantity);
        b.Ignore(s => s.IsLowStock);
    }
}

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> b)
    {
        b.HasKey(s => s.Id);
        b.HasIndex(s => new { s.TenantId, s.StoreId, s.ProductId, s.CreatedAt });

        b.Property(s => s.Quantity).HasPrecision(18, 4);
        b.Property(s => s.QuantityBefore).HasPrecision(18, 4);
        b.Property(s => s.QuantityAfter).HasPrecision(18, 4);
        b.Property(s => s.Type).HasConversion<string>().HasMaxLength(30);
        b.Property(s => s.Reference).HasMaxLength(100);
        b.Property(s => s.Notes).HasMaxLength(500);
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> b)
    {
        b.HasKey(p => p.Id);
        b.HasIndex(p => p.TransactionId);

        b.Property(p => p.Amount).HasPrecision(18, 4);
        b.Property(p => p.Method).HasConversion<string>().HasMaxLength(30);
        b.Property(p => p.PaymentStatus).HasConversion<string>().HasMaxLength(20);
        b.Property(p => p.GatewayReference).HasMaxLength(200);
        b.Property(p => p.GatewayResponse).HasMaxLength(1000);
        b.Property(p => p.MobileNumber).HasMaxLength(20);
        b.Property(p => p.DeclineReason).HasMaxLength(500);
    }
}

public class CreditAccountConfiguration : IEntityTypeConfiguration<CreditAccount>
{
    public void Configure(EntityTypeBuilder<CreditAccount> b)
    {
        b.HasKey(c => c.Id);
        b.HasIndex(c => new { c.TenantId, c.CustomerId, c.StoreId }).IsUnique();

        b.Property(c => c.CreditLimit).HasPrecision(18, 4);
        b.Property(c => c.OutstandingBalance).HasPrecision(18, 4).HasDefaultValue(0m);

        b.Ignore(c => c.AvailableCredit);

        b.HasMany(c => c.Transactions)
            .WithOne()
            .HasForeignKey(t => t.CreditAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CreditTransactionConfiguration : IEntityTypeConfiguration<CreditTransaction>
{
    public void Configure(EntityTypeBuilder<CreditTransaction> b)
    {
        b.HasKey(t => t.Id);
        b.HasIndex(t => t.CreditAccountId);

        b.Property(t => t.Amount).HasPrecision(18, 4);
        b.Property(t => t.Description).HasMaxLength(500).IsRequired();
        b.Property(t => t.Reference).HasMaxLength(200);
    }
}
