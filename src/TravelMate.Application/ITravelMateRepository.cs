using TravelMate.Domain;

namespace TravelMate.Application;

public interface ITravelMateRepository
{
    Task<IReadOnlyCollection<Place>> GetPlacesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Story>> GetStoriesAsync(CancellationToken cancellationToken);
    Task<Place> SavePlaceAsync(SavePlaceRequest request, CancellationToken cancellationToken);
    Task<Story> SaveStoryAsync(SaveStoryRequest request, CancellationToken cancellationToken);
    Task<Place?> FindPlaceByNameAsync(string placeName, CancellationToken cancellationToken);
    Task<UserPreference> GetPreferenceAsync(string userId, CancellationToken cancellationToken);
    Task SavePreferenceAsync(UserPreference preference, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PlaybackEvent>> GetPlaybackEventsAsync(string userId, CancellationToken cancellationToken);
    Task SavePlaybackEventAsync(PlaybackEvent playbackEvent, CancellationToken cancellationToken);
}

public sealed record SavePlaceRequest(
    Guid? Id,
    string Name,
    string Country,
    string Region,
    double Latitude,
    double Longitude,
    string[] Categories);

public sealed record SaveStoryRequest(
    Guid? Id,
    Guid PlaceId,
    string Title,
    string ShortDescription,
    string LanguageCode,
    string[] Categories,
    string SourceName,
    string SourceUrl,
    string? AudioUrl,
    int QualityScore);
