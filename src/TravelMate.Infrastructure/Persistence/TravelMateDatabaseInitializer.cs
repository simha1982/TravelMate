using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace TravelMate.Infrastructure.Persistence;

public sealed class TravelMateDatabaseInitializer(
    TravelMateDbContext dbContext,
    TravelMateSeeder seeder,
    IConfiguration configuration)
{
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (dbContext.Database.IsRelational()
            && configuration.GetValue("Database:ApplyMigrationsOnStartup", false))
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
        else if (!dbContext.Database.IsRelational())
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

        if (configuration.GetValue("TravelMate:SeedOnStartup", true))
        {
            await seeder.SeedAsync(cancellationToken);
        }
    }
}
