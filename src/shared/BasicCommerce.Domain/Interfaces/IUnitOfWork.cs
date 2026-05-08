namespace BasicCommerce.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IUserRefreshTokenRepository UserRefreshTokens { get; }
    IProductRepository Products { get; }
    IStockLevelRepository StockLevels { get; }
    ITransactionRepository Transactions { get; }
    IVatRateRepository VatRates { get; }
    IStoreRepository Stores { get; }
    ITerminalRepository Terminals { get; }
    ICategoryRepository Categories { get; }
    ICustomerRepository Customers { get; }
    ICreditAccountRepository CreditAccounts { get; }
    IStockMovementRepository StockMovements { get; }
    IManufacturerRepository Manufacturers { get; }
    ISupplierRepository Suppliers { get; }
    IWarehouseRepository Warehouses { get; }
    IWarehouseStockLevelRepository WarehouseStockLevels { get; }
    IWarehouseMovementRepository WarehouseMovements { get; }
    IPurchaseOrderRepository PurchaseOrders { get; }
    IRewardPointsSettingsRepository RewardPointsSettings { get; }
    IRewardPointsAccountRepository RewardPointsAccounts { get; }
    IProductTagRepository ProductTags { get; }
    IProductReviewRepository ProductReviews { get; }
    IStockBatchRepository StockBatches { get; }
    ISupplierReturnRepository SupplierReturns { get; }
    IGiftCardRepository GiftCards { get; }
    ITillSessionRepository TillSessions { get; }
    IPromotionRepository Promotions { get; }
    ILabelTemplateRepository LabelTemplates { get; }
    ILabelPrintJobRepository LabelPrintJobs { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
