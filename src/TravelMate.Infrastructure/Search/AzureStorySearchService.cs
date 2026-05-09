using Azure;
using Azure.Search.Documents;
using TravelMate.Application;

namespace TravelMate.Infrastructure.Search;

public sealed class AzureStorySearchService(StorySearchOptions options) : IStorySearchService
{
    public async Task IndexStoriesAsync(IReadOnlyCollection<SearchableStory> stories, CancellationToken cancellationToken)
    {
        var client = CreateClient();
        await client.MergeOrUploadDocumentsAsync(stories.Select(ToDocument), cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyCollection<SearchableStory>> SearchAsync(string query, int top, CancellationToken cancellationToken)
    {
        var client = CreateClient();
        var results = await client.SearchAsync<StorySearchDocument>(
            query,
            new Azure.Search.Documents.SearchOptions { Size = top <= 0 ? 3 : top },
            cancellationToken);

        var matches = new List<SearchableStory>();
        await foreach (var item in results.Value.GetResultsAsync())
        {
            matches.Add(ToSearchableStory(item.Document));
        }

        return matches;
    }

    private SearchClient CreateClient() =>
        new(new Uri(options.Endpoint), options.IndexName, new AzureKeyCredential(options.ApiKey));

    private static StorySearchDocument ToDocument(SearchableStory story) => new()
    {
        Id = story.Id,
        PlaceName = story.PlaceName,
        Title = story.Title,
        Summary = story.Summary,
        LanguageCode = story.LanguageCode,
        Categories = story.Categories,
        SourceName = story.SourceName,
        SourceUrl = story.SourceUrl
    };

    private static SearchableStory ToSearchableStory(StorySearchDocument document) => new(
        document.Id,
        document.PlaceName,
        document.Title,
        document.Summary,
        document.LanguageCode,
        document.Categories,
        document.SourceName,
        document.SourceUrl);

    private sealed class StorySearchDocument
    {
        public string Id { get; set; } = string.Empty;
        public string PlaceName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = "en";
        public string[] Categories { get; set; } = [];
        public string SourceName { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
    }
}
