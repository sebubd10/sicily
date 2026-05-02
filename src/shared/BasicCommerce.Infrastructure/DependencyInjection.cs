using BasicCommerce.Application.Interfaces;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using BasicCommerce.Infrastructure.Providers;
using BasicCommerce.Infrastructure.Repositories;
using BasicCommerce.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BasicCommerce.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDatabase(configuration);

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserRefreshTokenRepository, UserRefreshTokenRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IStockLevelRepository, StockLevelRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IVatRateRepository, VatRateRepository>();
        services.AddScoped<IStoreRepository, StoreRepository>();
        services.AddScoped<ITerminalRepository, TerminalRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICreditAccountRepository, CreditAccountRepository>();

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
