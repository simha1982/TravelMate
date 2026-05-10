namespace TravelMate.Domain;

public sealed record AiAuditEvent(
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
