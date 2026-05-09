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
