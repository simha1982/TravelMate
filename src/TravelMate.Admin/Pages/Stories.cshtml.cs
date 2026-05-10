using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TravelMate.Admin;

namespace TravelMate.Admin.Pages;

public sealed class StoriesModel(TravelMateApiClient apiClient) : PageModel
{
    public IReadOnlyCollection<StoryRow> Stories { get; private set; } = [];
    public IReadOnlyCollection<PlaceDto> Places { get; private set; } = [];
    public StoriesSummary Summary { get; private set; } = new(0, 0, 0);
    public string? StatusMessage { get; private set; }
    public string? ErrorMessage { get; private set; }

    [BindProperty]
    public CreatePlaceForm CreatePlace { get; set; } = new();

    [BindProperty]
    public CreateStoryForm CreateStory { get; set; } = new();

    [BindProperty]
    public EditStoryForm EditStory { get; set; } = new();

    [BindProperty]
    public EditPlaceForm EditPlace { get; set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        StatusMessage = TempData["StatusMessage"] as string;
        await LoadAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostCreatePlaceAsync(CancellationToken cancellationToken)
    {
        await apiClient.CreatePlaceAsync(new SavePlaceRequestDto(
            null,
            CreatePlace.Name,
            CreatePlace.Country,
            CreatePlace.Region,
            CreatePlace.Latitude,
            CreatePlace.Longitude,
            ParseCsv(CreatePlace.Categories)), cancellationToken);

        TempData["StatusMessage"] = "Place created.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCreateStoryAsync(CancellationToken cancellationToken)
    {
        await apiClient.CreateStoryAsync(ToSaveStoryRequest(null, CreateStory), cancellationToken);
        TempData["StatusMessage"] = "Story created.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateStoryAsync(CancellationToken cancellationToken)
    {
        await apiClient.UpdateStoryAsync(
            EditStory.Id,
            ToSaveStoryRequest(EditStory.Id, EditStory),
            cancellationToken);

        TempData["StatusMessage"] = "Story updated.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdatePlaceAsync(CancellationToken cancellationToken)
    {
        await apiClient.UpdatePlaceAsync(
            EditPlace.Id,
            new SavePlaceRequestDto(
                EditPlace.Id,
                EditPlace.Name,
                EditPlace.Country,
                EditPlace.Region,
                EditPlace.Latitude,
                EditPlace.Longitude,
                ParseCsv(EditPlace.Categories)),
            cancellationToken);

        TempData["StatusMessage"] = "Place updated.";
        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            Places = await apiClient.GetPlacesAsync(cancellationToken);
            var stories = await apiClient.GetStoriesAsync(cancellationToken);
            var placeLookup = Places.ToDictionary(place => place.Id);

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
                Places.Count,
                Stories.Count(item => item.Story.SourceName.Equals("Wikipedia", StringComparison.OrdinalIgnoreCase)));
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"TravelMate API is unavailable or returned an error: {ex.Message}";
        }
    }

    private static SaveStoryRequestDto ToSaveStoryRequest(Guid? id, StoryFormBase form) => new(
        id,
        form.PlaceId,
        form.Title,
        form.ShortDescription,
        form.LanguageCode,
        ParseCsv(form.Categories),
        form.SourceName,
        form.SourceUrl,
        string.IsNullOrWhiteSpace(form.AudioUrl) ? null : form.AudioUrl,
        form.QualityScore);

    private static string[] ParseCsv(string? value) =>
        (value ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
}

public sealed record StoryRow(StoryDto Story, PlaceDto? Place);

public sealed record StoriesSummary(int StoryCount, int PlaceCount, int WikipediaStoryCount);

public class CreatePlaceForm
{
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = "India";
    public string Region { get; set; } = "Hyderabad";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Categories { get; set; } = "history,culture";
}

public sealed class EditPlaceForm : CreatePlaceForm
{
    public Guid Id { get; set; }
}

public abstract class StoryFormBase
{
    public Guid PlaceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = "en";
    public string Categories { get; set; } = "history,culture";
    public string SourceName { get; set; } = "Curated pilot content";
    public string SourceUrl { get; set; } = "internal://admin/story";
    public string? AudioUrl { get; set; }
    public int QualityScore { get; set; } = 75;
}

public sealed class CreateStoryForm : StoryFormBase;

public sealed class EditStoryForm : StoryFormBase
{
    public Guid Id { get; set; }
}
