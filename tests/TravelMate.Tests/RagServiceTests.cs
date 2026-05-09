using TravelMate.AI;
using TravelMate.Application;
using TravelMate.Infrastructure.Search;

namespace TravelMate.Tests;

public sealed class RagServiceTests
{
    [Fact]
    public async Task AnswerAsync_UsesIndexedStorySource()
    {
        var search = new InMemoryStorySearchService();
        await search.IndexStoriesAsync([
            new SearchableStory(
                "story-1",
                "Nandi Hills",
                "The Hill Fort Above Bengaluru",
                "Nandi Hills has fort walls, temple history, and sunrise views.",
                "en",
                ["history", "nature"],
                "Curated pilot content",
                "internal://pilot/nandi-hills")
        ], CancellationToken.None);

        var service = new RagService(search, new StubModelGateway());
        var answer = await service.AnswerAsync(new RagAnswerRequest("Tell me about Nandi Hills"), CancellationToken.None);

        Assert.Contains("Nandi Hills", answer.Answer);
        Assert.Single(answer.Sources);
    }
}
