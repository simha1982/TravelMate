using System.Collections.Concurrent;
using TravelMate.Application;
using TravelMate.Domain;

namespace TravelMate.Infrastructure;

public sealed class InMemoryTravelMateRepository : ITravelMateRepository
{
    private static readonly Guid NandiHillsId = Guid.Parse("4d7df980-92e0-4ed7-a4a8-f98f82311101");
    private static readonly Guid CricketStadiumId = Guid.Parse("4d7df980-92e0-4ed7-a4a8-f98f82311102");
    private static readonly Guid StLuciaId = Guid.Parse("4d7df980-92e0-4ed7-a4a8-f98f82311103");

    private readonly ConcurrentDictionary<string, UserPreference> preferences = new();
    private readonly ConcurrentBag<PlaybackEvent> playbackEvents = [];

    private readonly IReadOnlyCollection<Place> places =
    [
        new Place(
            NandiHillsId,
            "Nandi Hills",
            "India",
            "Karnataka",
            new GeoPoint(13.3702, 77.6835),
            ["history", "nature", "architecture"]),
        new Place(
            CricketStadiumId,
            "M. Chinnaswamy Stadium",
            "India",
            "Bengaluru",
            new GeoPoint(12.9788, 77.5996),
            ["cricket", "sports", "culture"]),
        new Place(
            StLuciaId,
            "iSimangaliso Wetland Park",
            "South Africa",
            "KwaZulu-Natal",
            new GeoPoint(-28.0000, 32.4800),
            ["nature", "scubaDiving", "history"])
    ];

    private readonly IReadOnlyCollection<Story> stories =
    [
        new Story(
            Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd201"),
            NandiHillsId,
            "The Hill Fort Above Bengaluru",
            "Nandi Hills is known for its old fort walls, temple history, sunrise views, and stories connected to Tipu Sultan's era.",
            "en",
            ["history", "nature", "architecture"],
            "Curated pilot content",
            "internal://pilot/nandi-hills",
            null,
            88),
        new Story(
            Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd202"),
            CricketStadiumId,
            "A Cricket Landmark in the City",
            "This Bengaluru stadium is a major cricket venue and a good example of the app recognizing a traveller's sports interests.",
            "en",
            ["cricket", "sports"],
            "Curated pilot content",
            "internal://pilot/chinnaswamy",
            null,
            82),
        new Story(
            Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd203"),
            StLuciaId,
            "Wetlands, Coast, and Wildlife",
            "This coastal wetland region is a strong pilot fit for nature, wildlife, and diving-related storytelling.",
            "en",
            ["nature", "scubaDiving"],
            "Curated pilot content",
            "internal://pilot/st-lucia",
            null,
            84)
    ];

    public Task<IReadOnlyCollection<Place>> GetPlacesAsync(CancellationToken cancellationToken) =>
        Task.FromResult(places);

    public Task<IReadOnlyCollection<Story>> GetStoriesAsync(CancellationToken cancellationToken) =>
        Task.FromResult(stories);

    public Task<UserPreference> GetPreferenceAsync(string userId, CancellationToken cancellationToken)
    {
        var preference = preferences.GetOrAdd(userId, _ => new UserPreference(
            userId,
            new Dictionary<string, double>
            {
                ["history"] = 0.75,
                ["nature"] = 0.65,
                ["cricket"] = 0.50,
                ["scubaDiving"] = 0.50,
                ["architecture"] = 0.40
            },
            "en"));

        return Task.FromResult(preference);
    }

    public Task SavePreferenceAsync(UserPreference preference, CancellationToken cancellationToken)
    {
        preferences[preference.UserId] = preference;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<PlaybackEvent>> GetPlaybackEventsAsync(string userId, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<PlaybackEvent> events = playbackEvents
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.OccurredAt)
            .Take(50)
            .ToArray();

        return Task.FromResult(events);
    }

    public Task SavePlaybackEventAsync(PlaybackEvent playbackEvent, CancellationToken cancellationToken)
    {
        playbackEvents.Add(playbackEvent);
        return Task.CompletedTask;
    }
}
