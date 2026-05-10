using TravelMate.Domain;

namespace TravelMate.Application;

public interface IAiAuditRepository
{
    Task SaveAsync(AiAuditEvent auditEvent, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AiAuditEvent>> GetRecentAsync(int limit, CancellationToken cancellationToken);
}

public sealed class AiAuditOptions
{
    public decimal ChatCostPerThousandTokensUsd { get; set; }
    public decimal EmbeddingCostPerThousandTokensUsd { get; set; }
}
