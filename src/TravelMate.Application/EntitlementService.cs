using TravelMate.Domain;

namespace TravelMate.Application;

public sealed class EntitlementService(
    ISubscriptionRepository subscriptionRepository,
    ITravelMateRepository travelMateRepository)
{
    public async Task<EntitlementResult> GetEntitlementsAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetAsync(userId, cancellationToken);
        var playbackEvents = await travelMateRepository.GetPlaybackEventsAsync(userId, cancellationToken);
        var today = DateTimeOffset.UtcNow.Date;
        var storiesPlayedToday = playbackEvents.Count(playbackEvent =>
            playbackEvent.Action is PlaybackAction.Played or PlaybackAction.Completed
            && playbackEvent.OccurredAt.UtcDateTime.Date == today);

        var isActive = subscription.ExpiresAt is null || subscription.ExpiresAt > DateTimeOffset.UtcNow;
        var effectiveTier = isActive ? subscription.Tier : SubscriptionTier.Free;
        var dailyLimit = effectiveTier == SubscriptionTier.Free
            ? subscription.DailyStoryLimit
            : int.MaxValue;

        return new EntitlementResult(
            userId,
            effectiveTier,
            dailyLimit,
            storiesPlayedToday,
            storiesPlayedToday < dailyLimit);
    }

    public async Task<Subscription> SaveAsync(
        SaveSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        var limit = request.DailyStoryLimit ?? (request.Tier == SubscriptionTier.Free ? 5 : int.MaxValue);
        var subscription = new Subscription(request.UserId, request.Tier, request.ExpiresAt, limit);
        await subscriptionRepository.SaveAsync(subscription, cancellationToken);
        return subscription;
    }
}
