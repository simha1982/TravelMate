using Microsoft.AspNetCore.Mvc.RazorPages;
using TravelMate.Admin;

namespace TravelMate.Admin.Pages;

public sealed class AiUsageModel(TravelMateApiClient apiClient) : PageModel
{
    public IReadOnlyCollection<AiAuditEventDto> Events { get; private set; } = [];
    public AiUsageSummary Summary { get; private set; } = new(0, 0, 0, 0, 0, 0, 0);
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            Events = await apiClient.GetAiAuditEventsAsync(100, cancellationToken);
            Summary = BuildSummary(Events);
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"TravelMate API is unavailable or returned an error: {ex.Message}";
        }
    }

    private static AiUsageSummary BuildSummary(IReadOnlyCollection<AiAuditEventDto> events)
    {
        if (events.Count == 0)
        {
            return new AiUsageSummary(0, 0, 0, 0, 0, 0, 0);
        }

        var succeeded = events.Count(item => item.Succeeded);
        var failed = events.Count - succeeded;
        var totalTokens = events.Sum(item => item.EstimatedTokens);
        var totalCost = events.Sum(item => item.EstimatedCostUsd);
        var averageLatency = (int)Math.Round(events.Average(item => item.LatencyMilliseconds));
        var uniqueTasks = events.Select(item => item.TaskName).Distinct(StringComparer.OrdinalIgnoreCase).Count();

        return new AiUsageSummary(events.Count, succeeded, failed, totalTokens, totalCost, averageLatency, uniqueTasks);
    }
}

public sealed record AiUsageSummary(
    int TotalCalls,
    int Succeeded,
    int Failed,
    int TotalTokens,
    decimal TotalCostUsd,
    int AverageLatencyMilliseconds,
    int UniqueTasks);
