using Microsoft.EntityFrameworkCore;
using TravelMate.Application;
using TravelMate.Domain;

namespace TravelMate.Infrastructure.Persistence;

public sealed class EfUserConsentRepository(TravelMateDbContext dbContext) : IUserConsentRepository
{
    public async Task<UserConsent> GetAsync(string userId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.UserConsents.AsNoTracking()
            .FirstOrDefaultAsync(consent => consent.UserId == userId, cancellationToken);

        return entity is null
            ? new UserConsent(userId, false, false, false, DateTimeOffset.MinValue)
            : ToDomain(entity);
    }

    public async Task SaveAsync(UserConsent consent, CancellationToken cancellationToken)
    {
        var entity = await dbContext.UserConsents
            .FirstOrDefaultAsync(item => item.UserId == consent.UserId, cancellationToken);

        if (entity is null)
        {
            entity = new UserConsentEntity { UserId = consent.UserId };
            dbContext.UserConsents.Add(entity);
        }

        entity.LocationConsent = consent.LocationConsent;
        entity.VoiceConsent = consent.VoiceConsent;
        entity.PersonalizationConsent = consent.PersonalizationConsent;
        entity.UpdatedAt = consent.UpdatedAt;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static UserConsent ToDomain(UserConsentEntity entity) => new(
        entity.UserId,
        entity.LocationConsent,
        entity.VoiceConsent,
        entity.PersonalizationConsent,
        entity.UpdatedAt);
}
