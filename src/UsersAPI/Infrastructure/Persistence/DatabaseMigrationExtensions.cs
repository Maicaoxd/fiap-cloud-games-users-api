using Microsoft.EntityFrameworkCore;

namespace UsersAPI.Infrastructure.Persistence
{
    public static class DatabaseMigrationExtensions
    {
        public static async Task ApplyDatabaseMigrationsAsync(this WebApplication app)
        {
            var shouldApplyMigrations = app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup");

            if (!shouldApplyMigrations)
                return;

            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
            var logger = scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("DatabaseMigration");

            const int maxAttempts = 10;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    logger.LogInformation("Applying UsersAPI database migrations. Attempt {Attempt}/{MaxAttempts}.", attempt, maxAttempts);
                    await dbContext.Database.MigrateAsync();
                    logger.LogInformation("UsersAPI database migrations applied successfully.");
                    return;
                }
                catch (Exception exception) when (attempt < maxAttempts)
                {
                    logger.LogWarning(
                        exception,
                        "UsersAPI database migration failed. Retrying in 5 seconds. Attempt {Attempt}/{MaxAttempts}.",
                        attempt,
                        maxAttempts);

                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }

            await dbContext.Database.MigrateAsync();
        }
    }
}
