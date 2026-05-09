namespace TravelMate.Domain;

public sealed record UserConsent(
    string UserId,
    bool LocationConsent,
    bool VoiceConsent,
    bool PersonalizationConsent,
    DateTimeOffset UpdatedAt);
