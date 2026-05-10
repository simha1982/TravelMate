using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TravelMate.Application;
using TravelMate.Domain;

namespace TravelMate.Infrastructure.Persistence;

public sealed class EfTravelMateRepository(TravelMateDbContext dbContext) : ITravelMateRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyCollection<Place>> GetPlacesAsync(CancellationToken cancellationToken)
    {
        var places = await dbContext.Places.AsNoTracking().ToArrayAsync(cancellationToken);
        return places.Select(ToDomain).ToArray();
    }

    public async Task<IReadOnlyCollection<Story>> GetStoriesAsync(CancellationToken cancellationToken)
    {
        var stories = await dbContext.Stories.AsNoTracking().ToArrayAsync(cancellationToken);
        return stories.Select(ToDomain).ToArray();
    }

    public async Task<Place> SavePlaceAsync(SavePlaceRequest request, CancellationToken cancellationToken)
    {
        var id = request.Id.GetValueOrDefault(Guid.NewGuid());
        var entity = await dbContext.Places.FirstOrDefaultAsync(place => place.Id == id, cancellationToken);
        if (entity is null)
        {
            entity = new PlaceEntity { Id = id };
            dbContext.Places.Add(entity);
        }

        entity.Name = request.Name.Trim();
        entity.Country = request.Country.Trim();
        entity.Region = request.Region.Trim();
        entity.Latitude = request.Latitude;
        entity.Longitude = request.Longitude;
        entity.CategoriesJson = JsonSerializer.Serialize(NormalizeCategories(request.Categories), JsonOptions);

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDomain(entity);
    }

    public async Task<Story> SaveStoryAsync(SaveStoryRequest request, CancellationToken cancellationToken)
    {
        var id = request.Id.GetValueOrDefault(Guid.NewGuid());
        var entity = await dbContext.Stories.FirstOrDefaultAsync(story => story.Id == id, cancellationToken);
        if (entity is null)
        {
            entity = new StoryEntity { Id = id };
            dbContext.Stories.Add(entity);
        }

        entity.PlaceId = request.PlaceId;
        entity.Title = request.Title.Trim();
        entity.ShortDescription = request.ShortDescription.Trim();
        entity.LanguageCode = string.IsNullOrWhiteSpace(request.LanguageCode) ? "en" : request.LanguageCode.Trim();
        entity.CategoriesJson = JsonSerializer.Serialize(NormalizeCategories(request.Categories), JsonOptions);
        entity.SourceName = request.SourceName.Trim();
        entity.SourceUrl = request.SourceUrl.Trim();
        entity.AudioUrl = string.IsNullOrWhiteSpace(request.AudioUrl) ? null : request.AudioUrl.Trim();
        entity.QualityScore = request.QualityScore;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDomain(entity);
    }

    public async Task<Place?> FindPlaceByNameAsync(string placeName, CancellationToken cancellationToken)
    {
        var normalized = placeName.Trim();
        var entity = await dbContext.Places.AsNoTracking()
            .FirstOrDefaultAsync(place => place.Name == normalized, cancellationToken);

        return entity is null ? null : ToDomain(entity);
    }

    public async Task<UserPreference> GetPreferenceAsync(string userId, CancellationToken cancellationToken)
    {
        var preference = await dbContext.UserPreferences.AsNoTracking()
            .FirstOrDefaultAsync(item => item.UserId == userId, cancellationToken);

        if (preference is not null)
        {
            return ToDomain(preference);
        }

        return new UserPreference(
            userId,
            new Dictionary<string, double>
            {
                ["history"] = 0.75,
                ["nature"] = 0.65,
                ["cricket"] = 0.50,
                ["scubaDiving"] = 0.50,
                ["architecture"] = 0.40
            },
            "en");
    }

    public async Task SavePreferenceAsync(UserPreference preference, CancellationToken cancellationToken)
    {
        var entity = await dbContext.UserPreferences
            .FirstOrDefaultAsync(item => item.UserId == preference.UserId, cancellationToken);

        if (entity is null)
        {
            entity = new UserPreferenceEntity { UserId = preference.UserId };
            dbContext.UserPreferences.Add(entity);
        }

        entity.PreferredLanguageCode = preference.PreferredLanguageCode;
        entity.InterestsJson = JsonSerializer.Serialize(preference.Interests, JsonOptions);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<PlaybackEvent>> GetPlaybackEventsAsync(string userId, CancellationToken cancellationToken)
    {
        var events = await dbContext.PlaybackEvents.AsNoTracking()
            .Where(playbackEvent => playbackEvent.UserId == userId)
            .OrderByDescending(playbackEvent => playbackEvent.OccurredAt)
            .Take(50)
            .ToArrayAsync(cancellationToken);

        return events.Select(ToDomain).ToArray();
    }

    public async Task SavePlaybackEventAsync(PlaybackEvent playbackEvent, CancellationToken cancellationToken)
    {
        dbContext.PlaybackEvents.Add(new PlaybackEventEntity
        {
            UserId = playbackEvent.UserId,
            StoryId = playbackEvent.StoryId,
            Action = playbackEvent.Action.ToString(),
            OccurredAt = playbackEvent.OccurredAt
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Place ToDomain(PlaceEntity entity) => new(
        entity.Id,
        entity.Name,
        entity.Country,
        entity.Region,
        new GeoPoint(entity.Latitude, entity.Longitude),
        Deserialize<string[]>(entity.CategoriesJson));

    private static Story ToDomain(StoryEntity entity) => new(
        entity.Id,
        entity.PlaceId,
        entity.Title,
        entity.ShortDescription,
        entity.LanguageCode,
        Deserialize<string[]>(entity.CategoriesJson),
        entity.SourceName,
        entity.SourceUrl,
        entity.AudioUrl,
        entity.QualityScore);

    private static UserPreference ToDomain(UserPreferenceEntity entity) => new(
        entity.UserId,
        Deserialize<Dictionary<string, double>>(entity.InterestsJson),
        entity.PreferredLanguageCode);

    private static PlaybackEvent ToDomain(PlaybackEventEntity entity) => new(
        entity.UserId,
        entity.StoryId,
        Enum.TryParse<PlaybackAction>(entity.Action, out var action) ? action : PlaybackAction.Played,
        entity.OccurredAt);

    private static T Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, JsonOptions) ?? throw new InvalidOperationException("Stored JSON was invalid.");

    private static string[] NormalizeCategories(IEnumerable<string> categories) =>
        categories
            .Where(category => !string.IsNullOrWhiteSpace(category))
            .Select(category => category.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
}
