namespace TravelMate.Domain;

public sealed record Subscription(
    string UserId,
    SubscriptionTier Tier,
    DateTimeOffset? ExpiresAt,
    int DailyStoryLimit);

public enum SubscriptionTier
{
    Free,
    Premium,
    Partner
}
