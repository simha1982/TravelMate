using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TravelMate.Admin;

namespace TravelMate.Admin.Pages;

public sealed class IndexModel(TravelMateApiClient apiClient) : PageModel
{
    public IReadOnlyCollection<ModerationItem> Items { get; private set; } = [];
    public string? StatusMessage { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostApproveAsync(Guid contributionId, CancellationToken cancellationToken)
    {
        await apiClient.ApproveAsync(contributionId, cancellationToken);
        TempData["StatusMessage"] = "Contribution approved.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(Guid contributionId, CancellationToken cancellationToken)
    {
        await apiClient.RejectAsync(contributionId, cancellationToken);
        TempData["StatusMessage"] = "Contribution rejected.";
        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        StatusMessage = TempData["StatusMessage"] as string;

        try
        {
            var queue = await apiClient.GetModerationQueueAsync(cancellationToken);
            var items = new List<ModerationItem>();
            foreach (var contribution in queue.OrderBy(item => item.SubmittedAt))
            {
                var results = await apiClient.GetModerationResultsAsync(contribution.Id, cancellationToken);
                items.Add(new ModerationItem(contribution, results.OrderByDescending(item => item.ReviewedAt).ToArray()));
            }

            Items = items;
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"TravelMate API is unavailable or returned an error: {ex.Message}";
            Items = [];
        }
    }
}

public sealed record ModerationItem(
    ContributionDto Contribution,
    IReadOnlyCollection<ModerationResultDto> Results);
