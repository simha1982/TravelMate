using System.Net.Http.Json;
using TravelMate.Mobile.Models;

namespace TravelMate.Mobile.Services;

public sealed class TravelMateApiClient(HttpClient httpClient)
{
    private const string DemoUserId = "demo-user";

    public async Task<IReadOnlyCollection<NearbyStoryDto>> GetNearbyStoriesAsync(
        double latitude,
        double longitude,
        double radiusMeters,
        CancellationToken cancellationToken)
    {
        var uri = $"api/stories/nearby?userId={DemoUserId}&lat={latitude}&lon={longitude}&radiusMeters={radiusMeters}";
        return await httpClient.GetFromJsonAsync<NearbyStoryDto[]>(uri, cancellationToken) ?? [];
    }

    public async Task SavePlaybackEventAsync(
        Guid storyId,
        string action,
        CancellationToken cancellationToken)
    {
        var request = new PlaybackEventRequest(DemoUserId, action);
        using var response = await httpClient.PostAsJsonAsync(
            $"api/stories/{storyId}/playback-events",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<RagAnswerResponse?> AskAsync(string question, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/rag/answer",
            new RagAnswerRequest(question, DemoUserId),
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RagAnswerResponse>(cancellationToken);
    }
}
