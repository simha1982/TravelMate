using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelMate.Application;
using TravelMate.Infrastructure.Persistence;

namespace TravelMate.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTravelMateInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TravelMateSql");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<TravelMateDbContext>(options =>
                options.UseInMemoryDatabase("TravelMate"));
        }
        else
        {
            services.AddDbContext<TravelMateDbContext>(options =>
                options.UseSqlServer(connectionString));
        }

        services.AddScoped<ITravelMateRepository, EfTravelMateRepository>();
        services.AddScoped<TravelMateSeeder>();
        return services;
    }
}
