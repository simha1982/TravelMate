using TravelMate.Domain;

namespace TravelMate.Application;

public sealed class NearbyStoryService(ITravelMateRepository repository)
{
    public async Task<IReadOnlyCollection<NearbyStoryResult>> GetNearbyStoriesAsync(
        string userId,
        GeoPoint userLocation,
        double radiusMeters,
        CancellationToken cancellationToken)
    {
        var places = await repository.GetPlacesAsync(cancellationToken);
        var stories = await repository.GetStoriesAsync(cancellationToken);
        var preference = await repository.GetPreferenceAsync(userId, cancellationToken);
        var playbackEvents = await repository.GetPlaybackEventsAsync(userId, cancellationToken);
        var recentlySkipped = playbackEvents
            .Where(e => e.Action is PlaybackAction.Skipped or PlaybackAction.NotInterested)
            .Select(e => e.StoryId)
            .ToHashSet();

        var query =
            from place in places
            let distance = place.Location.DistanceToMeters(userLocation)
            where distance <= radiusMeters
            join story in stories on place.Id equals story.PlaceId
            where story.LanguageCode == preference.PreferredLanguageCode
            where !recentlySkipped.Contains(story.Id)
            let score = ScoreStory(story, preference, distance)
            orderby score descending, distance
            select new NearbyStoryResult(
                story.Id,
                place.Id,
                place.Name,
                story.Title,
                story.ShortDescription,
                story.LanguageCode,
                Math.Round(distance, 0),
                score,
                story.AudioUrl,
                story.SourceName);

        return query.Take(5).ToArray();
    }

    private static double ScoreStory(Story story, UserPreference preference, double distanceMeters)
    {
        var interestScore = story.Categories
            .Select(category => preference.Interests.TryGetValue(category, out var weight) ? weight : 0.25)
            .DefaultIfEmpty(0.25)
            .Max();

        var distanceScore = Math.Max(0, 1 - distanceMeters / 10_000);
        var qualityScore = story.QualityScore / 100d;

        return Math.Round((interestScore * 0.55) + (distanceScore * 0.25) + (qualityScore * 0.20), 4);
    }
}

public sealed record NearbyStoryResult(
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
