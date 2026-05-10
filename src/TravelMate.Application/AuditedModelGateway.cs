using System.Diagnostics;
using TravelMate.AI;
using TravelMate.Domain;

namespace TravelMate.Application;

public sealed class AuditedModelGateway(
    IModelGateway inner,
    IAiAuditRepository auditRepository,
    AiAuditOptions options) : IModelGateway
{
    public async Task<AiResponse<T>> CompleteJsonAsync<T>(
        string taskName,
        object input,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await inner.CompleteJsonAsync<T>(taskName, input, cancellationToken);
            await SaveAuditAsync(taskName, "chat-completion", response.Model, response.EstimatedTokens, stopwatch, true, null, cancellationToken);
            return response;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await SaveAuditAsync(taskName, "chat-completion", "unknown", 0, stopwatch, false, ex.Message, cancellationToken);
            throw;
        }
    }

    public async Task<float[]> CreateEmbeddingAsync(string text, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var embedding = await inner.CreateEmbeddingAsync(text, cancellationToken);
            var estimatedTokens = EstimateTokens(text);
            await SaveAuditAsync("embedding", "embedding", "embedding", estimatedTokens, stopwatch, true, null, cancellationToken);
            return embedding;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await SaveAuditAsync("embedding", "embedding", "unknown", 0, stopwatch, false, ex.Message, cancellationToken);
            throw;
        }
    }

    private async Task SaveAuditAsync(
        string taskName,
        string operation,
        string model,
        int estimatedTokens,
        Stopwatch stopwatch,
        bool succeeded,
        string? errorMessage,
        CancellationToken cancellationToken)
    {
        stopwatch.Stop();
        var estimatedCost = operation == "embedding"
            ? CalculateCost(estimatedTokens, options.EmbeddingCostPerThousandTokensUsd)
            : CalculateCost(estimatedTokens, options.ChatCostPerThousandTokensUsd);
        var auditEvent = new AiAuditEvent(
            Guid.NewGuid(),
            taskName,
            operation,
            model,
            estimatedTokens,
            estimatedCost,
            (int)Math.Min(int.MaxValue, stopwatch.ElapsedMilliseconds),
            succeeded,
            errorMessage,
            DateTimeOffset.UtcNow);

        await auditRepository.SaveAsync(auditEvent, cancellationToken);
    }

    private static int EstimateTokens(string text) =>
        string.IsNullOrWhiteSpace(text) ? 0 : Math.Max(1, (int)Math.Ceiling(text.Length / 4.0));

    private static decimal CalculateCost(int estimatedTokens, decimal costPerThousandTokensUsd) =>
        costPerThousandTokensUsd <= 0 || estimatedTokens <= 0
            ? 0
            : Math.Round(estimatedTokens / 1000m * costPerThousandTokensUsd, 6);
}
