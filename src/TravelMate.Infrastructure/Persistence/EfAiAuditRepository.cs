using Microsoft.EntityFrameworkCore;
using TravelMate.Application;
using TravelMate.Domain;

namespace TravelMate.Infrastructure.Persistence;

public sealed class EfAiAuditRepository(TravelMateDbContext dbContext) : IAiAuditRepository
{
    public async Task SaveAsync(AiAuditEvent auditEvent, CancellationToken cancellationToken)
    {
        dbContext.AiAuditEvents.Add(new AiAuditEventEntity
        {
            Id = auditEvent.Id,
            TaskName = auditEvent.TaskName,
            Operation = auditEvent.Operation,
            Model = auditEvent.Model,
            EstimatedTokens = auditEvent.EstimatedTokens,
            EstimatedCostUsd = auditEvent.EstimatedCostUsd,
            LatencyMilliseconds = auditEvent.LatencyMilliseconds,
            Succeeded = auditEvent.Succeeded,
            ErrorMessage = auditEvent.ErrorMessage,
            OccurredAt = auditEvent.OccurredAt
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<AiAuditEvent>> GetRecentAsync(int limit, CancellationToken cancellationToken)
    {
        var take = Math.Clamp(limit, 1, 200);
        var events = await dbContext.AiAuditEvents.AsNoTracking()
            .OrderByDescending(item => item.OccurredAt)
            .Take(take)
            .ToArrayAsync(cancellationToken);

        return events.Select(ToDomain).ToArray();
    }

    private static AiAuditEvent ToDomain(AiAuditEventEntity entity) => new(
        entity.Id,
        entity.TaskName,
        entity.Operation,
        entity.Model,
        entity.EstimatedTokens,
        entity.EstimatedCostUsd,
        entity.LatencyMilliseconds,
        entity.Succeeded,
        entity.ErrorMessage,
        entity.OccurredAt);
}
