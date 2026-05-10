using System.Net.Http.Json;

namespace TravelMate.Admin;

public sealed class TravelMateApiClient(HttpClient httpClient, IConfiguration configuration)
{
    public async Task<IReadOnlyCollection<PlaceDto>> GetPlacesAsync(CancellationToken cancellationToken)
    {
        return await httpClient.GetFromJsonAsync<PlaceDto[]>(
            "/api/places",
            cancellationToken) ?? [];
    }

    public async Task<IReadOnlyCollection<StoryDto>> GetStoriesAsync(CancellationToken cancellationToken)
    {
        return await httpClient.GetFromJsonAsync<StoryDto[]>(
            "/api/stories",
            cancellationToken) ?? [];
    }

    public async Task<IReadOnlyCollection<AiAuditEventDto>> GetAiAuditEventsAsync(
        int limit,
        CancellationToken cancellationToken)
    {
        return await httpClient.GetFromJsonAsync<AiAuditEventDto[]>(
            $"/api/admin/ai-audit?limit={limit}",
            cancellationToken) ?? [];
    }

    public async Task<IReadOnlyCollection<ContributionDto>> GetModerationQueueAsync(CancellationToken cancellationToken)
    {
        return await httpClient.GetFromJsonAsync<ContributionDto[]>(
            "/api/admin/moderation-queue",
            cancellationToken) ?? [];
    }

    public async Task<IReadOnlyCollection<ModerationResultDto>> GetModerationResultsAsync(
        Guid contributionId,
        CancellationToken cancellationToken)
    {
        return await httpClient.GetFromJsonAsync<ModerationResultDto[]>(
            $"/api/admin/contributions/{contributionId}/moderation-results",
            cancellationToken) ?? [];
    }

    public async Task<ContributionDto?> ApproveAsync(Guid contributionId, CancellationToken cancellationToken)
    {
        using var request = CreatePost($"/api/admin/contributions/{contributionId}/approve");
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ContributionDto>(cancellationToken);
    }

    public async Task<ContributionDto?> RejectAsync(Guid contributionId, CancellationToken cancellationToken)
    {
        using var request = CreatePost($"/api/admin/contributions/{contributionId}/reject");
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ContributionDto>(cancellationToken);
    }

    private HttpRequestMessage CreatePost(string path)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path);
        var apiKey = configuration["TravelMateApi:ApiKey"];
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            request.Headers.Add("X-TravelMate-Api-Key", apiKey);
        }

        return request;
    }
}

public sealed record ContributionDto(
    Guid Id,
    string ContributorUserId,
    string PlaceName,
    GeoPointDto Location,
    string LanguageCode,
    string Title,
    string StoryText,
    string Status,
    DateTimeOffset SubmittedAt);

public sealed record GeoPointDto(double Latitude, double Longitude);

public sealed record PlaceDto(
    Guid Id,
    string Name,
    string Country,
    string Region,
    GeoPointDto Location,
    string[] Categories);

public sealed record StoryDto(
    Guid Id,
    Guid PlaceId,
    string Title,
    string ShortDescription,
    string LanguageCode,
    string[] Categories,
    string SourceName,
    string SourceUrl,
    string? AudioUrl,
    int QualityScore);

public sealed record ModerationResultDto(
    Guid Id,
    Guid ContributionId,
    bool Passed,
    string Summary,
    string[] Flags,
    DateTimeOffset ReviewedAt);

public sealed record AiAuditEventDto(
    Guid Id,
    string TaskName,
    string Operation,
    string Model,
    int EstimatedTokens,
    decimal EstimatedCostUsd,
    int LatencyMilliseconds,
    bool Succeeded,
    string? ErrorMessage,
    DateTimeOffset OccurredAt);
