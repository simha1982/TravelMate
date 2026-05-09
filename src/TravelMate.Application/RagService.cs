using TravelMate.AI;

namespace TravelMate.Application;

public sealed class RagService(IStorySearchService searchService, IModelGateway modelGateway)
{
    public async Task<RagAnswerResponse> AnswerAsync(
        RagAnswerRequest request,
        CancellationToken cancellationToken)
    {
        var sources = await searchService.SearchAsync(request.Question, request.Top <= 0 ? 3 : request.Top, cancellationToken);
        if (sources.Count == 0)
        {
            return new RagAnswerResponse(
                "I could not find a trusted TravelMate story source for that question yet.",
                []);
        }

        var aiResponse = await modelGateway.CompleteJsonAsync<RagTextResponse>(
            "travelmate-rag-answer",
            new
            {
                request.Question,
                Sources = sources.Select(source => new
                {
                    source.PlaceName,
                    source.Title,
                    source.Summary,
                    source.SourceName,
                    source.SourceUrl
                })
            },
            cancellationToken);

        var answer = string.IsNullOrWhiteSpace(aiResponse.Value.Answer)
            ? BuildFallbackAnswer(request.Question, sources)
            : aiResponse.Value.Answer;

        return new RagAnswerResponse(answer, sources.ToArray());
    }

    private static string BuildFallbackAnswer(string question, IReadOnlyCollection<SearchableStory> sources)
    {
        var top = sources.First();
        return $"Based on TravelMate's pilot content, {top.PlaceName}: {top.Summary}";
    }

    private sealed class RagTextResponse
    {
        public string Answer { get; init; } = string.Empty;
    }
}
