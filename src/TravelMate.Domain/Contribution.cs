namespace TravelMate.Domain;

public sealed record Contribution(
    Guid Id,
    string ContributorUserId,
    string PlaceName,
    GeoPoint Location,
    string LanguageCode,
    string Title,
    string StoryText,
    ContributionStatus Status,
    DateTimeOffset SubmittedAt);

public enum ContributionStatus
{
    Submitted,
    NeedsChanges,
    Approved,
    Rejected
}

public sealed record ModerationResult(
    Guid Id,
    Guid ContributionId,
    bool Passed,
    string Summary,
    IReadOnlyCollection<string> Flags,
    DateTimeOffset ReviewedAt);
