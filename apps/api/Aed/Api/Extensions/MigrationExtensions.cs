using Infrastructure.Database;
using Infrastructure.Identity;
using Infrastructure.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedKernel;

namespace Api.Extensions;

/// <summary>
/// Applies EF migrations and seeds the database on API start (every environment).
/// </summary>
public static class MigrationExtensions
{
    /// <summary>
    /// Waits for the database, applies pending migrations and runs the seeders.
    /// </summary>
    /// <param name="app">Web application instance.</param>
    public static async Task ApplyMigrationsAndSeedAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        IServiceProvider services = scope.ServiceProvider;
        ApplicationDbContext dbContext = services.GetRequiredService<ApplicationDbContext>();
        SeedOptions seedOptions = services.GetRequiredService<IOptions<SeedOptions>>().Value;
        DateTime utcNow = services.GetRequiredService<IDateTimeProvider>().UtcNow;
        ILogger logger = app.Logger;

        // From the configuration: the context's own copy has the password removed.
        await WaitForDatabaseAsync(
            app.Configuration.GetConnectionString(Infrastructure.DependencyInjection.DatabaseConnectionName)!,
            logger);

        // Destructive switch: never outside Development, whatever the configuration says.
        if (app.Environment.IsDevelopment() && seedOptions.ResetDatabase)
        {
            logger.LogWarning("Seed:ResetDatabase is enabled: dropping and recreating the database.");
            await dbContext.Database.EnsureDeletedAsync();
            Npgsql.NpgsqlConnection.ClearAllPools();
        }

        await dbContext.Database.MigrateAsync();

        await RoleSeeder.SeedAsync(services.GetRequiredService<RoleManager<IdentityRole<Guid>>>());
        if (seedOptions.DemoData)
        {
            await DemoDataSeeder.SeedAsync(dbContext, utcNow, logger);
        }

        await UserSeeder.SeedAsync(
            services.GetRequiredService<UserManager<ApplicationUser>>(),
            seedOptions,
            utcNow,
            logger);
    }

    private static async Task WaitForDatabaseAsync(string connectionString, ILogger logger)
    {
        const int maxAttempts = 30;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                // The database itself may not exist yet (MigrateAsync creates it): ask the server only.
                await using var connection = new Npgsql.NpgsqlConnection(
                    new Npgsql.NpgsqlConnectionStringBuilder(connectionString)
                    {
                        Database = "postgres"
                    }.ConnectionString);
                await connection.OpenAsync();
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                logger.LogWarning(
                    ex,
                    "Waiting for the database server, attempt {Attempt}/{MaxAttempts}.",
                    attempt,
                    maxAttempts);
            }

            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }
}
