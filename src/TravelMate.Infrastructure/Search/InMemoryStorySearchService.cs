using System.Collections.Concurrent;
using TravelMate.Application;

namespace TravelMate.Infrastructure.Search;

public sealed class InMemoryStorySearchService : IStorySearchService
{
    private readonly ConcurrentDictionary<string, SearchableStory> stories = new();

    public Task IndexStoriesAsync(IReadOnlyCollection<SearchableStory> storiesToIndex, CancellationToken cancellationToken)
    {
        foreach (var story in storiesToIndex)
        {
            stories[story.Id] = story;
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<SearchableStory>> SearchAsync(string query, int top, CancellationToken cancellationToken)
    {
        var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var results = stories.Values
            .Select(story => new
            {
                Story = story,
                Score = terms.Sum(term => Score(story, term))
            })
            .Where(item => item.Score > 0 || terms.Length == 0)
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Story.PlaceName)
            .Take(top <= 0 ? 3 : top)
            .Select(item => item.Story)
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<SearchableStory>>(results);
    }

    private static int Score(SearchableStory story, string term)
    {
        return Contains(story.PlaceName, term) * 4
            + Contains(story.Title, term) * 3
            + Contains(story.Summary, term) * 2
            + story.Categories.Sum(category => Contains(category, term));
    }

    private static int Contains(string value, string term) =>
        value.Contains(term, StringComparison.OrdinalIgnoreCase) ? 1 : 0;
}
