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
        var ignoredStories = playbackEvents
            .Where(e => e.Action is PlaybackAction.NotInterested)
            .Select(e => e.StoryId)
            .ToHashSet();
        var recentlySkipped = playbackEvents
            .Where(e => e.Action is PlaybackAction.Skipped)
            .Where(e => e.OccurredAt >= DateTimeOffset.UtcNow.AddDays(-3))
            .Select(e => e.StoryId)
            .ToHashSet();
        var recentlyPlayed = playbackEvents
            .Where(e => e.Action is PlaybackAction.Played or PlaybackAction.Completed)
            .Where(e => e.OccurredAt >= DateTimeOffset.UtcNow.AddDays(-7))
            .Select(e => e.StoryId)
            .ToHashSet();
        var categorySignals = BuildCategorySignals(stories, playbackEvents);

        var query =
            from place in places
            let distance = place.Location.DistanceToMeters(userLocation)
            where distance <= radiusMeters
            join story in stories on place.Id equals story.PlaceId
            where story.LanguageCode == preference.PreferredLanguageCode
            where !ignoredStories.Contains(story.Id)
            let score = ScoreStory(
                story,
                preference,
                categorySignals,
                distance,
                recentlyPlayed.Contains(story.Id),
                recentlySkipped.Contains(story.Id))
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

    private static double ScoreStory(
        Story story,
        UserPreference preference,
        IReadOnlyDictionary<string, double> categorySignals,
        double distanceMeters,
        bool wasRecentlyPlayed,
        bool wasRecentlySkipped)
    {
        var interestScore = story.Categories
            .Select(category =>
            {
                var preferenceWeight = preference.Interests.TryGetValue(category, out var weight) ? weight : 0.25;
                var behaviorSignal = categorySignals.TryGetValue(category, out var signal) ? signal : 0;
                return Math.Clamp(preferenceWeight + behaviorSignal, 0, 1);
            })
            .DefaultIfEmpty(0.25)
            .Max();

        var distanceScore = Math.Max(0, 1 - distanceMeters / 10_000);
        var qualityScore = story.QualityScore / 100d;

        var playbackPenalty = (wasRecentlyPlayed ? 0.35 : 0) + (wasRecentlySkipped ? 0.20 : 0);
        return Math.Round(Math.Max(0, (interestScore * 0.55) + (distanceScore * 0.25) + (qualityScore * 0.20) - playbackPenalty), 4);
    }

    private static IReadOnlyDictionary<string, double> BuildCategorySignals(
        IReadOnlyCollection<Story> stories,
        IReadOnlyCollection<PlaybackEvent> playbackEvents)
    {
        var storiesById = stories.ToDictionary(story => story.Id);
        var signals = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        var recentEvents = playbackEvents
            .Where(playbackEvent => playbackEvent.OccurredAt >= DateTimeOffset.UtcNow.AddDays(-30))
            .OrderByDescending(playbackEvent => playbackEvent.OccurredAt)
            .Take(25);

        foreach (var playbackEvent in recentEvents)
        {
            if (!storiesById.TryGetValue(playbackEvent.StoryId, out var story))
            {
                continue;
            }

            var delta = playbackEvent.Action switch
            {
                PlaybackAction.Interested or PlaybackAction.Saved or PlaybackAction.Replayed => 0.10,
                PlaybackAction.Completed => 0.06,
                PlaybackAction.Skipped => -0.05,
                _ => 0
            };

            if (delta == 0)
            {
                continue;
            }

            foreach (var category in story.Categories)
            {
                signals[category] = Math.Clamp(signals.GetValueOrDefault(category) + delta, -0.25, 0.25);
            }
        }

        return signals;
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
