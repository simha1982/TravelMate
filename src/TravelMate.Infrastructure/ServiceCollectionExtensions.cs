using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelMate.Application;
using TravelMate.Infrastructure.Persistence;
using TravelMate.Infrastructure.Search;
using TravelMate.Infrastructure.Storage;

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
        services.AddScoped<IContributionRepository, EfContributionRepository>();
        services.AddScoped<ISubscriptionRepository, EfSubscriptionRepository>();
        services.AddScoped<IUserConsentRepository, EfUserConsentRepository>();
        services.AddScoped<TravelMateSeeder>();
        services.AddScoped<TravelMateDatabaseInitializer>();

        var audioOptions = configuration.GetSection("AudioStorage").Get<AudioStorageOptions>() ?? new AudioStorageOptions();
        services.AddSingleton(audioOptions);
        services.AddScoped<IAudioStorageService>(_ =>
            audioOptions.IsAzureConfigured
                ? new AzureBlobAudioStorageService(audioOptions)
                : new LocalAudioStorageService(audioOptions));

        var searchOptions = configuration.GetSection("AzureSearch").Get<StorySearchOptions>() ?? new StorySearchOptions();
        services.AddSingleton(searchOptions);
        if (searchOptions.IsAzureConfigured)
        {
            services.AddScoped<IStorySearchService>(_ => new AzureStorySearchService(searchOptions));
        }
        else
        {
            services.AddSingleton<IStorySearchService, InMemoryStorySearchService>();
        }

        return services;
    }
}
