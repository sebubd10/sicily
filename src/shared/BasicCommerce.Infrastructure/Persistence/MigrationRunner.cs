using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BasicCommerce.Infrastructure.Persistence;

public static class MigrationRunner
{
    /// <summary>
    /// Applies pending EF Core migrations and seeds reference data.
    /// Controlled by appsettings.json:
    ///   "Startup": { "RunMigrations": true, "SeedData": true }
    /// </summary>
    public static async Task MigrateAndSeedAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp     = scope.ServiceProvider;
        var logger = sp.GetRequiredService<ILogger<BasicCommerceDbContext>>();
        var config = sp.GetRequiredService<IConfiguration>();

        var runMigrations = config.GetValue("Startup:RunMigrations", defaultValue: true);
        var seedData      = config.GetValue("Startup:SeedData",      defaultValue: true);

        try
        {
            if (runMigrations)
            {
                var db = sp.GetRequiredService<BasicCommerceDbContext>();
                logger.LogInformation("Applying pending migrations…");
                await db.Database.MigrateAsync();
                logger.LogInformation("Migrations applied.");
            }
            else
            {
                logger.LogInformation("Startup:RunMigrations is false — skipping migrations.");
            }

            if (seedData)
            {
                var seeder = sp.GetRequiredService<DatabaseSeeder>();
                await seeder.SeedAsync();
            }
            else
            {
                logger.LogInformation("Startup:SeedData is false — skipping seed.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Migration/seed failed. Application startup aborted.");
            throw;
        }
    }
}
