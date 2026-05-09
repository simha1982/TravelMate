namespace TravelMate.Domain;

public sealed record PlaybackEvent(
    string UserId,
    Guid StoryId,
    PlaybackAction Action,
    DateTimeOffset OccurredAt);

public enum PlaybackAction
{
    Played,
    Completed,
    Interested,
    NotInterested,
    Skipped,
    Saved,
    Replayed
}
