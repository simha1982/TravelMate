using Microsoft.EntityFrameworkCore;
using TravelMate.Application;
using TravelMate.Domain;
using TravelMate.Infrastructure.Persistence;

namespace TravelMate.Tests;

public sealed class EntitlementServiceTests
{
    [Fact]
    public async Task GetEntitlementsAsync_ReturnsFalseWhenFreeDailyLimitIsReached()
    {
        await using var dbContext = CreateDbContext();
        var travelMateRepository = new EfTravelMateRepository(dbContext);
        var subscriptionRepository = new EfSubscriptionRepository(dbContext);
        var service = new EntitlementService(subscriptionRepository, travelMateRepository);
        var userId = "free-user";

        await subscriptionRepository.SaveAsync(
            new Subscription(userId, SubscriptionTier.Free, null, 1),
            CancellationToken.None);
        await travelMateRepository.SavePlaybackEventAsync(
            new PlaybackEvent(userId, Guid.NewGuid(), PlaybackAction.Played, DateTimeOffset.UtcNow),
            CancellationToken.None);

        var entitlement = await service.GetEntitlementsAsync(userId, CancellationToken.None);

        Assert.Equal(SubscriptionTier.Free, entitlement.Tier);
        Assert.Equal(1, entitlement.DailyStoryLimit);
        Assert.Equal(1, entitlement.StoriesPlayedToday);
        Assert.False(entitlement.CanPlayMoreStories);
    }

    private static TravelMateDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TravelMateDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new TravelMateDbContext(options);
    }
}
