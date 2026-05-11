namespace TravelMate.Mobile.Models;

public sealed record NearbyStoryDto(
    Guid StoryId,
    Guid PlaceId,
    string PlaceName,
    string Title,
    string ShortDescription,
    string LanguageCode,
    double DistanceMeters,
    double Score,
    string? AudioUrl,
    string SourceName);

public sealed record PlaybackEventRequest(string UserId, string Action);

public sealed record SpeechSynthesisRequest(
    string Text,
    string LanguageCode = "en-US",
    string VoiceName = "en-US-JennyNeural");

public sealed record StoredAudio(string Url, string ContentType, long SizeBytes);

public sealed record RagAnswerRequest(string Question, string UserId = "demo-user", int Top = 3);

public sealed record RagAnswerResponse(string Answer, SearchableStoryDto[] Sources);

public sealed record SearchableStoryDto(
    string Id,
    string PlaceName,
    string Title,
    string Summary,
    string LanguageCode,
    string[] Categories,
    string SourceName,
    string SourceUrl);

public sealed record UserPreferenceRequest(
    string UserId,
    IReadOnlyDictionary<string, double> Interests,
    string PreferredLanguageCode);

public sealed record SaveUserConsentRequest(
    bool LocationConsent,
    bool VoiceConsent,
    bool PersonalizationConsent);

public sealed record SubmitContributionRequest(
    string ContributorUserId,
    string PlaceName,
    double Latitude,
    double Longitude,
    string LanguageCode,
    string Title,
    string StoryText);

public sealed record StoryDetailDto(StoryDto Story, PlaceDto? Place);

public sealed record StoryDto(
    Guid Id,
    Guid PlaceId,
    string Title,
    string ShortDescription,
    string LanguageCode,
    string[] Categories,
    string SourceName,
    string SourceUrl,
    string? AudioUrl,
    int QualityScore);

public sealed record PlaceDto(
    Guid Id,
    string Name,
    string Country,
    string Region,
    GeoPointDto Location,
    string[] Categories);

public sealed record GeoPointDto(double Latitude, double Longitude);

public sealed record HealthResponse(string Status, HealthCheckDto[] Checks);

public sealed record HealthCheckDto(string Name, string Status, string? Detail);
