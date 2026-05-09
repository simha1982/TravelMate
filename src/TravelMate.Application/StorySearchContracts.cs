namespace TravelMate.Application;

public interface IStorySearchService
{
    Task IndexStoriesAsync(IReadOnlyCollection<SearchableStory> stories, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<SearchableStory>> SearchAsync(string query, int top, CancellationToken cancellationToken);
}

public sealed record SearchableStory(
    string Id,
    string PlaceName,
    string Title,
    string Summary,
    string LanguageCode,
    string[] Categories,
    string SourceName,
    string SourceUrl);

public sealed record RagAnswerRequest(
    string Question,
    string UserId = "demo-user",
    int Top = 3);

public sealed record RagAnswerResponse(
    string Answer,
    SearchableStory[] Sources);
