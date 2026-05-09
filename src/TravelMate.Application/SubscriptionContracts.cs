using TravelMate.Domain;

namespace TravelMate.Application;

public interface ISubscriptionRepository
{
    Task<Subscription> GetAsync(string userId, CancellationToken cancellationToken);
    Task SaveAsync(Subscription subscription, CancellationToken cancellationToken);
}

public sealed record SaveSubscriptionRequest(
    string UserId,
    SubscriptionTier Tier,
    DateTimeOffset? ExpiresAt,
    int? DailyStoryLimit);

public sealed record EntitlementResult(
    string UserId,
    SubscriptionTier Tier,
    int DailyStoryLimit,
    int StoriesPlayedToday,
    bool CanPlayMoreStories);
