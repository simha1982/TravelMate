namespace TravelMate.Domain;

public sealed record UserPreference(
    string UserId,
    IReadOnlyDictionary<string, double> Interests,
    string PreferredLanguageCode);
