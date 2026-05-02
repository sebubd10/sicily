using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BasicCommerce.Infrastructure.Persistence;

public static class MigrationRunner
{
    /// <summary>
    /// Applies any pending EF Core migrations and seeds reference data.
    /// Call from Program.cs after app.Build() and before app.Run().
    /// </summary>
    public static async Task MigrateAndSeedAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILogger<BasicCommerceDbContext>>();

        try
        {
            var db = scope.ServiceProvider
                .GetRequiredService<BasicCommerceDbContext>();

            logger.LogInformation("Applying pending migrations…");
            await db.Database.MigrateAsync();
            logger.LogInformation("Migrations applied.");

            var seeder = scope.ServiceProvider
                .GetRequiredService<DatabaseSeeder>();
            await seeder.SeedAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Migration/seed failed. Application startup aborted.");
            throw;
        }
    }
}
