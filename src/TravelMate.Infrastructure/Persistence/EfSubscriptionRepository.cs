using Microsoft.EntityFrameworkCore;
using TravelMate.Application;
using TravelMate.Domain;

namespace TravelMate.Infrastructure.Persistence;

public sealed class EfSubscriptionRepository(TravelMateDbContext dbContext) : ISubscriptionRepository
{
    private const int FreeDailyStoryLimit = 5;

    public async Task<Subscription> GetAsync(string userId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Subscriptions.AsNoTracking()
            .FirstOrDefaultAsync(subscription => subscription.UserId == userId, cancellationToken);

        return entity is null
            ? new Subscription(userId, SubscriptionTier.Free, null, FreeDailyStoryLimit)
            : ToDomain(entity);
    }

    public async Task SaveAsync(Subscription subscription, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Subscriptions
            .FirstOrDefaultAsync(item => item.UserId == subscription.UserId, cancellationToken);

        if (entity is null)
        {
            entity = new SubscriptionEntity { UserId = subscription.UserId };
            dbContext.Subscriptions.Add(entity);
        }

        entity.Tier = subscription.Tier.ToString();
        entity.ExpiresAt = subscription.ExpiresAt;
        entity.DailyStoryLimit = subscription.DailyStoryLimit;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Subscription ToDomain(SubscriptionEntity entity) => new(
        entity.UserId,
        Enum.TryParse<SubscriptionTier>(entity.Tier, out var tier) ? tier : SubscriptionTier.Free,
        entity.ExpiresAt,
        entity.DailyStoryLimit);
}
