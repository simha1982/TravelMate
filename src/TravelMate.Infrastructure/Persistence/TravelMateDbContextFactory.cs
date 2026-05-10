using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TravelMate.Infrastructure.Persistence;

public sealed class TravelMateDbContextFactory : IDesignTimeDbContextFactory<TravelMateDbContext>
{
    public TravelMateDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("TRAVELMATE_MIGRATION_CONNECTION")
            ?? "Server=(localdb)\\mssqllocaldb;Database=TravelMateDesignTime;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<TravelMateDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new TravelMateDbContext(options);
    }
}
