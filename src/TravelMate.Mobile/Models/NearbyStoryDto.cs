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
