using Microsoft.EntityFrameworkCore;
using TravelMate.Application;
using TravelMate.Domain;
using TravelMate.Infrastructure.Persistence;

namespace TravelMate.Tests;

public sealed class SyntheticRouteTests
{
    [Fact]
    public async Task PilotRoute_ReturnsExpectedStoriesAtKnownWaypoints()
    {
        await using var dbContext = CreateDbContext();
        await new TravelMateSeeder(dbContext).SeedAsync(CancellationToken.None);
        var service = new NearbyStoryService(new EfTravelMateRepository(dbContext));
        var route = new[]
        {
            new RouteWaypoint("Nandi Hills approach", new GeoPoint(13.3702, 77.6835), "The Hill Fort Above Bengaluru"),
            new RouteWaypoint("Bengaluru cricket district", new GeoPoint(12.9788, 77.5996), "A Cricket Landmark in the City"),
            new RouteWaypoint("St Lucia wetlands", new GeoPoint(-28.0000, 32.4800), "Wetlands, Coast, and Wildlife")
        };

        foreach (var waypoint in route)
        {
            var results = await service.GetNearbyStoriesAsync(
                "synthetic-route-user",
                waypoint.Location,
                5_000,
                CancellationToken.None);

            var topResult = Assert.Single(results);
            Assert.Equal(waypoint.ExpectedStoryTitle, topResult.Title);
        }
    }

    [Fact]
    public async Task PilotRoute_DoesNotRepeatRejectedWaypointStory()
    {
        await using var dbContext = CreateDbContext();
        await new TravelMateSeeder(dbContext).SeedAsync(CancellationToken.None);
        var repository = new EfTravelMateRepository(dbContext);
        var service = new NearbyStoryService(repository);
        var userId = "synthetic-route-user";
        var nandiStoryId = Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd201");

        await repository.SavePlaybackEventAsync(
            new PlaybackEvent(userId, nandiStoryId, PlaybackAction.NotInterested, DateTimeOffset.UtcNow),
            CancellationToken.None);

        var results = await service.GetNearbyStoriesAsync(
            userId,
            new GeoPoint(13.3702, 77.6835),
            5_000,
            CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task HyderabadRoute_ReturnsWikipediaSourcedStories()
    {
        await using var dbContext = CreateDbContext();
        await new TravelMateSeeder(dbContext).SeedAsync(CancellationToken.None);
        var service = new NearbyStoryService(new EfTravelMateRepository(dbContext));

        var results = await service.GetNearbyStoriesAsync(
            "hyderabad-route-user",
            new GeoPoint(17.3616, 78.4747),
            12_000,
            CancellationToken.None);

        Assert.Contains(results, story => story.PlaceName == "Charminar" && story.SourceName == "Wikipedia");
        Assert.Contains(results, story => story.PlaceName == "Chowmahalla Palace" && story.SourceName == "Wikipedia");
        Assert.Contains(results, story => story.PlaceName == "Salar Jung Museum" && story.SourceName == "Wikipedia");
    }

    private static TravelMateDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TravelMateDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new TravelMateDbContext(options);
    }

    private sealed record RouteWaypoint(string Name, GeoPoint Location, string ExpectedStoryTitle);
}
