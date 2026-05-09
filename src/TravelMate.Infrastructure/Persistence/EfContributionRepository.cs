using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TravelMate.Application;
using TravelMate.Domain;

namespace TravelMate.Infrastructure.Persistence;

public sealed class EfContributionRepository(TravelMateDbContext dbContext) : IContributionRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Contribution> SubmitAsync(Contribution contribution, CancellationToken cancellationToken)
    {
        dbContext.Contributions.Add(ToEntity(contribution));
        await dbContext.SaveChangesAsync(cancellationToken);
        return contribution;
    }

    public async Task<IReadOnlyCollection<Contribution>> GetQueueAsync(CancellationToken cancellationToken)
    {
        var queue = await dbContext.Contributions.AsNoTracking()
            .Where(contribution => contribution.Status == ContributionStatus.Submitted.ToString()
                || contribution.Status == ContributionStatus.NeedsChanges.ToString())
            .OrderBy(contribution => contribution.SubmittedAt)
            .ToArrayAsync(cancellationToken);

        return queue.Select(ToDomain).ToArray();
    }

    public async Task<Contribution?> GetAsync(Guid contributionId, CancellationToken cancellationToken)
    {
        var contribution = await dbContext.Contributions.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == contributionId, cancellationToken);

        return contribution is null ? null : ToDomain(contribution);
    }

    public async Task<Contribution?> UpdateStatusAsync(
        Guid contributionId,
        ContributionStatus status,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.Contributions
            .FirstOrDefaultAsync(item => item.Id == contributionId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.Status = status.ToString();
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDomain(entity);
    }

    public async Task SaveModerationResultAsync(ModerationResult result, CancellationToken cancellationToken)
    {
        dbContext.ModerationResults.Add(new ModerationResultEntity
        {
            Id = result.Id,
            ContributionId = result.ContributionId,
            Passed = result.Passed,
            Summary = result.Summary,
            FlagsJson = JsonSerializer.Serialize(result.Flags, JsonOptions),
            ReviewedAt = result.ReviewedAt
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ModerationResult>> GetModerationResultsAsync(
        Guid contributionId,
        CancellationToken cancellationToken)
    {
        var results = await dbContext.ModerationResults.AsNoTracking()
            .Where(result => result.ContributionId == contributionId)
            .OrderByDescending(result => result.ReviewedAt)
            .ToArrayAsync(cancellationToken);

        return results.Select(ToDomain).ToArray();
    }

    private static ContributionEntity ToEntity(Contribution contribution) => new()
    {
        Id = contribution.Id,
        ContributorUserId = contribution.ContributorUserId,
        PlaceName = contribution.PlaceName,
        Latitude = contribution.Location.Latitude,
        Longitude = contribution.Location.Longitude,
        LanguageCode = contribution.LanguageCode,
        Title = contribution.Title,
        StoryText = contribution.StoryText,
        Status = contribution.Status.ToString(),
        SubmittedAt = contribution.SubmittedAt
    };

    private static Contribution ToDomain(ContributionEntity entity) => new(
        entity.Id,
        entity.ContributorUserId,
        entity.PlaceName,
        new GeoPoint(entity.Latitude, entity.Longitude),
        entity.LanguageCode,
        entity.Title,
        entity.StoryText,
        Enum.TryParse<ContributionStatus>(entity.Status, out var status) ? status : ContributionStatus.Submitted,
        entity.SubmittedAt);

    private static ModerationResult ToDomain(ModerationResultEntity entity) => new(
        entity.Id,
        entity.ContributionId,
        entity.Passed,
        entity.Summary,
        JsonSerializer.Deserialize<string[]>(entity.FlagsJson, JsonOptions) ?? [],
        entity.ReviewedAt);
}
