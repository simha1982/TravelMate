using Microsoft.EntityFrameworkCore;
using TravelMate.Application;
using TravelMate.Domain;
using TravelMate.Infrastructure.Persistence;

namespace TravelMate.Tests;

public sealed class NearbyStoryServiceTests
{
    [Fact]
    public async Task NearbyStories_ReturnsNandiHillsForMatchingCoordinates()
    {
        await using var dbContext = CreateDbContext();
        await new TravelMateSeeder(dbContext).SeedAsync(CancellationToken.None);
        var service = new NearbyStoryService(new EfTravelMateRepository(dbContext));

        var results = await service.GetNearbyStoriesAsync(
            "demo-user",
            new GeoPoint(13.3702, 77.6835),
            5_000,
            CancellationToken.None);

        var topResult = Assert.Single(results);
        Assert.Equal("Nandi Hills", topResult.PlaceName);
        Assert.Equal("The Hill Fort Above Bengaluru", topResult.Title);
    }

    [Fact]
    public async Task NearbyStories_ExcludesStoriesRejectedByUser()
    {
        await using var dbContext = CreateDbContext();
        await new TravelMateSeeder(dbContext).SeedAsync(CancellationToken.None);
        var repository = new EfTravelMateRepository(dbContext);
        var storyId = Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd201");

        await repository.SavePlaybackEventAsync(
            new PlaybackEvent("demo-user", storyId, PlaybackAction.NotInterested, DateTimeOffset.UtcNow),
            CancellationToken.None);

        var service = new NearbyStoryService(repository);
        var results = await service.GetNearbyStoriesAsync(
            "demo-user",
            new GeoPoint(13.3702, 77.6835),
            5_000,
            CancellationToken.None);

        Assert.Empty(results);
    }

    private static TravelMateDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TravelMateDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new TravelMateDbContext(options);
    }
}
