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
