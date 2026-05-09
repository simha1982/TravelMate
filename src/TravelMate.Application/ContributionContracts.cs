using TravelMate.Domain;

namespace TravelMate.Application;

public interface IContributionRepository
{
    Task<Contribution> SubmitAsync(Contribution contribution, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Contribution>> GetQueueAsync(CancellationToken cancellationToken);
    Task<Contribution?> GetAsync(Guid contributionId, CancellationToken cancellationToken);
    Task<Contribution?> UpdateStatusAsync(Guid contributionId, ContributionStatus status, CancellationToken cancellationToken);
    Task SaveModerationResultAsync(ModerationResult result, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ModerationResult>> GetModerationResultsAsync(Guid contributionId, CancellationToken cancellationToken);
}

public sealed record SubmitContributionRequest(
    string ContributorUserId,
    string PlaceName,
    double Latitude,
    double Longitude,
    string LanguageCode,
    string Title,
    string StoryText);

public sealed class ModerationReview
{
    public bool Passed { get; init; }
    public string Summary { get; init; } = string.Empty;
    public string[] Flags { get; init; } = [];
}
