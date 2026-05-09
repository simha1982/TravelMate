using TravelMate.Domain;

namespace TravelMate.Application;

public interface ITravelMateRepository
{
    Task<IReadOnlyCollection<Place>> GetPlacesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Story>> GetStoriesAsync(CancellationToken cancellationToken);
    Task<UserPreference> GetPreferenceAsync(string userId, CancellationToken cancellationToken);
    Task SavePreferenceAsync(UserPreference preference, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PlaybackEvent>> GetPlaybackEventsAsync(string userId, CancellationToken cancellationToken);
    Task SavePlaybackEventAsync(PlaybackEvent playbackEvent, CancellationToken cancellationToken);
}
