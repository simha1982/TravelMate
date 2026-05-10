using TravelMate.AI;
using TravelMate.Application;
using TravelMate.Domain;

namespace TravelMate.Tests;

public sealed class AuditedModelGatewayTests
{
    [Fact]
    public async Task CompleteJsonAsync_SavesSuccessfulAuditEvent()
    {
        var repository = new RecordingAuditRepository();
        var gateway = new AuditedModelGateway(
            new SuccessfulGateway(),
            repository,
            new AiAuditOptions { ChatCostPerThousandTokensUsd = 0.01m });

        var response = await gateway.CompleteJsonAsync<StorySummaryResponse>(
            "travel-story-summary",
            new { placeName = "Nandi Hills" },
            CancellationToken.None);

        var auditEvent = Assert.Single(repository.Events);
        Assert.True(auditEvent.Succeeded);
        Assert.Equal("travel-story-summary", auditEvent.TaskName);
        Assert.Equal("chat-completion", auditEvent.Operation);
        Assert.Equal("stub-model", auditEvent.Model);
        Assert.Equal(42, auditEvent.EstimatedTokens);
        Assert.Equal(0.00042m, auditEvent.EstimatedCostUsd);
        Assert.Equal("ok", response.Value.Title);
    }

    [Fact]
    public async Task CompleteJsonAsync_SavesFailedAuditEvent()
    {
        var repository = new RecordingAuditRepository();
        var gateway = new AuditedModelGateway(new FailingGateway(), repository, new AiAuditOptions());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            gateway.CompleteJsonAsync<StorySummaryResponse>(
                "travel-story-summary",
                new { placeName = "Nandi Hills" },
                CancellationToken.None));

        var auditEvent = Assert.Single(repository.Events);
        Assert.False(auditEvent.Succeeded);
        Assert.Equal("unknown", auditEvent.Model);
        Assert.Contains("model failed", auditEvent.ErrorMessage);
    }

    private sealed class RecordingAuditRepository : IAiAuditRepository
    {
        public List<AiAuditEvent> Events { get; } = [];

        public Task SaveAsync(AiAuditEvent auditEvent, CancellationToken cancellationToken)
        {
            Events.Add(auditEvent);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<AiAuditEvent>> GetRecentAsync(int limit, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<AiAuditEvent>>(Events.Take(limit).ToArray());
        }
    }

    private sealed class SuccessfulGateway : IModelGateway
    {
        public Task<AiResponse<T>> CompleteJsonAsync<T>(string taskName, object input, CancellationToken cancellationToken)
        {
            object value = new StorySummaryResponse("ok", "summary", ["history"], []);
            return Task.FromResult(new AiResponse<T>((T)value, "stub-model", 42));
        }

        public Task<float[]> CreateEmbeddingAsync(string text, CancellationToken cancellationToken)
        {
            return Task.FromResult(new[] { 0.1f, 0.2f });
        }
    }

    private sealed class FailingGateway : IModelGateway
    {
        public Task<AiResponse<T>> CompleteJsonAsync<T>(string taskName, object input, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("model failed");
        }

        public Task<float[]> CreateEmbeddingAsync(string text, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("embedding failed");
        }
    }
}
