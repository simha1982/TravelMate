using Microsoft.AspNetCore.Mvc.RazorPages;
using TravelMate.Admin;

namespace TravelMate.Admin.Pages;

public sealed class StoriesModel(TravelMateApiClient apiClient) : PageModel
{
    public IReadOnlyCollection<StoryRow> Stories { get; private set; } = [];
    public StoriesSummary Summary { get; private set; } = new(0, 0, 0);
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            var places = await apiClient.GetPlacesAsync(cancellationToken);
            var stories = await apiClient.GetStoriesAsync(cancellationToken);
            var placeLookup = places.ToDictionary(place => place.Id);

            Stories = stories
                .OrderBy(story => placeLookup.TryGetValue(story.PlaceId, out var place) ? place.Region : string.Empty)
                .ThenBy(story => placeLookup.TryGetValue(story.PlaceId, out var place) ? place.Name : string.Empty)
                .ThenBy(story => story.Title)
                .Select(story =>
                {
                    placeLookup.TryGetValue(story.PlaceId, out var place);
                    return new StoryRow(story, place);
                })
                .ToArray();

            Summary = new StoriesSummary(
                Stories.Count,
                places.Count,
                Stories.Count(item => item.Story.SourceName.Equals("Wikipedia", StringComparison.OrdinalIgnoreCase)));
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"TravelMate API is unavailable or returned an error: {ex.Message}";
        }
    }
}

public sealed record StoryRow(StoryDto Story, PlaceDto? Place);

public sealed record StoriesSummary(int StoryCount, int PlaceCount, int WikipediaStoryCount);
