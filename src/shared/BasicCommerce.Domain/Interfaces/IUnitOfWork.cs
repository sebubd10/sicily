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

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
