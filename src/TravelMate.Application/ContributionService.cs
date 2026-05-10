using TravelMate.AI;
using TravelMate.Domain;

namespace TravelMate.Application;

public sealed class ContributionService(
    IContributionRepository repository,
    ITravelMateRepository travelMateRepository,
    IModelGateway modelGateway)
{
    public async Task<Contribution> SubmitAsync(
        SubmitContributionRequest request,
        CancellationToken cancellationToken)
    {
        var contribution = new Contribution(
            Guid.NewGuid(),
            request.ContributorUserId,
            request.PlaceName,
            new GeoPoint(request.Latitude, request.Longitude),
            request.LanguageCode,
            request.Title,
            request.StoryText,
            ContributionStatus.Submitted,
            DateTimeOffset.UtcNow);

        var saved = await repository.SubmitAsync(contribution, cancellationToken);
        var review = await ReviewAsync(saved, cancellationToken);
        await repository.SaveModerationResultAsync(new ModerationResult(
            Guid.NewGuid(),
            saved.Id,
            review.Passed,
            review.Summary,
            review.Flags,
            DateTimeOffset.UtcNow), cancellationToken);

        return saved;
    }

    public Task<IReadOnlyCollection<Contribution>> GetQueueAsync(CancellationToken cancellationToken) =>
        repository.GetQueueAsync(cancellationToken);

    public Task<Contribution?> UpdateStatusAsync(
        Guid contributionId,
        ContributionStatus status,
        CancellationToken cancellationToken) =>
        repository.UpdateStatusAsync(contributionId, status, cancellationToken);

    public async Task<PublishedContributionResult?> ApproveAndPublishAsync(
        Guid contributionId,
        CancellationToken cancellationToken)
    {
        var contribution = await repository.GetAsync(contributionId, cancellationToken);
        if (contribution is null)
        {
            return null;
        }

        var place = await travelMateRepository.FindPlaceByNameAsync(contribution.PlaceName, cancellationToken)
            ?? await travelMateRepository.SavePlaceAsync(new SavePlaceRequest(
                null,
                contribution.PlaceName,
                "Unknown",
                contribution.PlaceName,
                contribution.Location.Latitude,
                contribution.Location.Longitude,
                ["community", "local"]), cancellationToken);

        var story = await travelMateRepository.SaveStoryAsync(new SaveStoryRequest(
            null,
            place.Id,
            contribution.Title,
            contribution.StoryText,
            contribution.LanguageCode,
            ["community", "local"],
            "Community contribution",
            $"contribution://{contribution.Id:N}",
            null,
            70), cancellationToken);

        var updated = await repository.UpdateStatusAsync(contributionId, ContributionStatus.Approved, cancellationToken)
            ?? contribution with { Status = ContributionStatus.Approved };

        return new PublishedContributionResult(updated, story);
    }

    public Task<IReadOnlyCollection<ModerationResult>> GetModerationResultsAsync(
        Guid contributionId,
        CancellationToken cancellationToken) =>
        repository.GetModerationResultsAsync(contributionId, cancellationToken);

    private async Task<ModerationReview> ReviewAsync(Contribution contribution, CancellationToken cancellationToken)
    {
        var response = await modelGateway.CompleteJsonAsync<ModerationReview>(
            "travelmate-contribution-moderation",
            new
            {
                contribution.PlaceName,
                contribution.LanguageCode,
                contribution.Title,
                contribution.StoryText,
                Checks = new[]
                {
                    "factual consistency",
                    "offensive language",
                    "cultural sensitivity",
                    "dangerous or misleading claims",
                    "unsupported historical claims"
                }
            },
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(response.Value.Summary))
        {
            return response.Value;
        }

        var flags = new List<string>();
        if (contribution.StoryText.Length < 40)
        {
            flags.Add("Story is too short for publication.");
        }

        return new ModerationReview
        {
            Passed = flags.Count == 0,
            Summary = flags.Count == 0
                ? "Local moderation stub found no obvious issue. Human review is still required before publishing."
                : "Local moderation stub found issues that need review.",
            Flags = flags.ToArray()
        };
    }
}

public sealed record PublishedContributionResult(Contribution Contribution, Story Story);
