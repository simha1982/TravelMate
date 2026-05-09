using TravelMate.Domain;

namespace TravelMate.Application;

public interface IUserConsentRepository
{
    Task<UserConsent> GetAsync(string userId, CancellationToken cancellationToken);
    Task SaveAsync(UserConsent consent, CancellationToken cancellationToken);
}

public sealed record SaveUserConsentRequest(
    bool LocationConsent,
    bool VoiceConsent,
    bool PersonalizationConsent);
