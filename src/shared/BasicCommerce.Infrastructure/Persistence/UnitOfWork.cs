using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace BasicCommerce.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly BasicCommerceDbContext _db;
    private IDbContextTransaction? _transaction;

    public IUserRepository Users { get; }
    public IUserRefreshTokenRepository UserRefreshTokens { get; }
    public IProductRepository Products { get; }
    public IStockLevelRepository StockLevels { get; }
    public ITransactionRepository Transactions { get; }
    public IVatRateRepository VatRates { get; }
    public IStoreRepository Stores { get; }
    public ITerminalRepository Terminals { get; }
    public ICategoryRepository Categories { get; }
    public ICustomerRepository Customers { get; }
    public ICreditAccountRepository CreditAccounts { get; }
    public IStockMovementRepository StockMovements { get; }
    public IManufacturerRepository Manufacturers { get; }
    public ISupplierRepository Suppliers { get; }
    public IWarehouseRepository Warehouses { get; }
    public IWarehouseStockLevelRepository WarehouseStockLevels { get; }
    public IWarehouseMovementRepository WarehouseMovements { get; }
    public IPurchaseOrderRepository PurchaseOrders { get; }
    public IRewardPointsSettingsRepository RewardPointsSettings { get; }
    public IRewardPointsAccountRepository RewardPointsAccounts { get; }
    public IProductTagRepository ProductTags { get; }
    public IProductReviewRepository ProductReviews { get; }
    public IStockBatchRepository StockBatches { get; }

    public UnitOfWork(BasicCommerceDbContext db)
    {
        _db = db;
        Users = new UserRepository(db);
        UserRefreshTokens = new UserRefreshTokenRepository(db);
        Products = new ProductRepository(db);
        StockLevels = new StockLevelRepository(db);
        Transactions = new TransactionRepository(db);
        VatRates = new VatRateRepository(db);
        Stores = new StoreRepository(db);
        Terminals = new TerminalRepository(db);
        Categories = new CategoryRepository(db);
        Customers = new CustomerRepository(db);
        CreditAccounts = new CreditAccountRepository(db);
        StockMovements = new StockMovementRepository(db);
        Manufacturers = new ManufacturerRepository(db);
        Suppliers = new SupplierRepository(db);
        Warehouses = new WarehouseRepository(db);
        WarehouseStockLevels = new WarehouseStockLevelRepository(db);
        WarehouseMovements = new WarehouseMovementRepository(db);
        PurchaseOrders = new PurchaseOrderRepository(db);
        RewardPointsSettings = new RewardPointsSettingsRepository(db);
        RewardPointsAccounts = new RewardPointsAccountRepository(db);
        ProductTags = new ProductTagRepository(db);
        ProductReviews = new ProductReviewRepository(db);
        StockBatches = new StockBatchRepository(db);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _db.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default) =>
        _transaction = await _db.Database.BeginTransactionAsync(ct);

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is null) return;
        await _transaction.CommitAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is null) return;
        await _transaction.RollbackAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _db.Dispose();
    }
}
