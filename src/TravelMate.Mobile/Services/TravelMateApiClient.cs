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

    public async Task<StoredAudio?> GenerateStoryAudioAsync(
        NearbyStoryDto story,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync(
            $"api/stories/{story.StoryId}/audio",
            new SpeechSynthesisRequest(story.ShortDescription, NormalizeLanguage(story.LanguageCode)),
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<StoredAudio>(cancellationToken);
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

    public async Task SavePreferencesAsync(
        IReadOnlyDictionary<string, double> interests,
        string preferredLanguageCode,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/preferences",
            new UserPreferenceRequest(DemoUserId, interests, preferredLanguageCode),
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task SaveConsentAsync(
        bool locationConsent,
        bool voiceConsent,
        bool personalizationConsent,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync(
            $"api/users/{DemoUserId}/consents",
            new SaveUserConsentRequest(locationConsent, voiceConsent, personalizationConsent),
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<string> SynthesizeToLocalFileAsync(
        string text,
        string languageCode,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/speech/synthesize",
            new SpeechSynthesisRequest(text, NormalizeLanguage(languageCode)),
            cancellationToken);
        response.EnsureSuccessStatusCode();

        var extension = response.Content.Headers.ContentType?.MediaType?.Contains("mpeg", StringComparison.OrdinalIgnoreCase) == true
            ? "mp3"
            : "txt";
        var fileName = $"travelmate-answer-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.{extension}";
        var path = Path.Combine(FileSystem.CacheDirectory, fileName);
        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        await File.WriteAllBytesAsync(path, bytes, cancellationToken);
        return path;
    }

    private static string NormalizeLanguage(string languageCode) =>
        languageCode.Equals("en", StringComparison.OrdinalIgnoreCase) ? "en-US" : languageCode;
}
