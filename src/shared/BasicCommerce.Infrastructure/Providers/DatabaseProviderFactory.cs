using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BasicCommerce.Infrastructure.Providers;

public static class DatabaseProviderFactory
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var providerName = configuration["Database:Provider"]
            ?? throw new InvalidOperationException("Database:Provider is not configured.");

        var connectionString = configuration["Database:ConnectionString"]
            ?? throw new InvalidOperationException("Database:ConnectionString is not configured.");

        if (!Enum.TryParse<DatabaseProvider>(providerName, ignoreCase: true, out var provider))
            throw new InvalidOperationException(
                $"Unknown database provider '{providerName}'. " +
                $"Supported: SqlServer, PostgreSql, MySql, Oracle.");

        services.AddDbContext<BasicCommerceDbContext>(options =>
        {
            _ = provider switch
            {
                DatabaseProvider.SqlServer =>
                    options.UseSqlServer(connectionString, sql =>
                        sql.MigrationsAssembly("BasicCommerce.Infrastructure")),

                DatabaseProvider.PostgreSql =>
                    options.UseNpgsql(connectionString, npg =>
                        npg.MigrationsAssembly("BasicCommerce.Infrastructure")),

                DatabaseProvider.MySql =>
                    options.UseMySql(connectionString,
                        ServerVersion.AutoDetect(connectionString), my =>
                        my.MigrationsAssembly("BasicCommerce.Infrastructure")),

                DatabaseProvider.Oracle =>
                    options.UseOracle(connectionString, ora =>
                        ora.MigrationsAssembly("BasicCommerce.Infrastructure")),

                _ => throw new InvalidOperationException($"Unhandled provider: {provider}")
            };
        });

        return services;
    }
}
