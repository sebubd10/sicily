using System.Linq.Expressions;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Persistence;

public class BasicCommerceDbContext : DbContext
{
    public BasicCommerceDbContext(DbContextOptions<BasicCommerceDbContext> options)
        : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Terminal> Terminals => Set<Terminal>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<VatRate> VatRates => Set<VatRate>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<LineItem> LineItems => Set<LineItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<StockLevel> StockLevels => Set<StockLevel>();
    public DbSet<CreditAccount> CreditAccounts => Set<CreditAccount>();
    public DbSet<CreditTransaction> CreditTransactions => Set<CreditTransaction>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<UserRefreshToken> UserRefreshTokens => Set<UserRefreshToken>();
    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<WarehouseStockLevel> WarehouseStockLevels => Set<WarehouseStockLevel>();
    public DbSet<WarehouseMovement> WarehouseMovements => Set<WarehouseMovement>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<RewardPointsSettings> RewardPointsSettings => Set<RewardPointsSettings>();
    public DbSet<RewardPointsAccount> RewardPointsAccounts => Set<RewardPointsAccount>();
    public DbSet<RewardPointsEntry> RewardPointsEntries => Set<RewardPointsEntry>();
    public DbSet<ProductTag> ProductTags => Set<ProductTag>();
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
    public DbSet<ProductReviewDetail> ProductReviewDetails => Set<ProductReviewDetail>();
    public DbSet<ProductReviewHelpfulness> ProductReviewHelpfulnesses => Set<ProductReviewHelpfulness>();
    public DbSet<StockBatch> StockBatches => Set<StockBatch>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(BasicCommerceDbContext).Assembly);
        ApplyEntityStatusFilter(builder);
    }

    private static void ApplyEntityStatusFilter(ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes()
            .Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType) && !t.IsOwned()))
        {
            var param = Expression.Parameter(entityType.ClrType, "e");
            var statusProp = Expression.Property(param, nameof(BaseEntity.Status));
            var notDeleted = Expression.NotEqual(
                statusProp,
                Expression.Constant(EntityStatus.Deleted));
            entityType.SetQueryFilter(Expression.Lambda(notDeleted, param));

            builder.Entity(entityType.ClrType)
                .Property<EntityStatus>(nameof(BaseEntity.Status))
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(EntityStatus.Active);
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(ct);
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
            entry.Property(nameof(BaseEntity.UpdatedAt)).CurrentValue = DateTime.UtcNow;
    }
}
