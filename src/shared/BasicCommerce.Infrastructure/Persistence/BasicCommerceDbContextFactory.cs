using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BasicCommerce.Infrastructure.Persistence;

/// <summary>
/// Used by EF Core CLI tools (dotnet ef migrations add / database update).
/// Reads appsettings.json from the Auth.Api project as the design-time host.
/// </summary>
public class BasicCommerceDbContextFactory : IDesignTimeDbContextFactory<BasicCommerceDbContext>
{
    public BasicCommerceDbContext CreateDbContext(string[] args)
    {
        // Walk up from Infrastructure to find appsettings.json in Auth.Api
        var basePath = Path.Combine(Directory.GetCurrentDirectory(),
            "..", "..", "..", "services", "BasicCommerce.Auth.Api");

        if (!Directory.Exists(basePath))
            basePath = Directory.GetCurrentDirectory();

        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var provider = config["Database:Provider"] ?? "PostgreSql";
        var connectionString = config["Database:ConnectionString"]
            ?? "Host=localhost;Database=basiccommerce;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<BasicCommerceDbContext>();

        switch (provider.ToLowerInvariant())
        {
            case "sqlserver":
                optionsBuilder.UseSqlServer(connectionString,
                    sql => sql.MigrationsAssembly("BasicCommerce.Infrastructure"));
                break;
            case "mysql":
                optionsBuilder.UseMySql(connectionString,
                    ServerVersion.AutoDetect(connectionString),
                    my => my.MigrationsAssembly("BasicCommerce.Infrastructure"));
                break;
            case "oracle":
                optionsBuilder.UseOracle(connectionString,
                    ora => ora.MigrationsAssembly("BasicCommerce.Infrastructure"));
                break;
            default: // postgresql
                optionsBuilder.UseNpgsql(connectionString,
                    npg => npg.MigrationsAssembly("BasicCommerce.Infrastructure"));
                break;
        }

        return new BasicCommerceDbContext(optionsBuilder.Options);
    }
}
