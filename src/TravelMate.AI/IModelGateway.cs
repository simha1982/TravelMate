namespace TravelMate.AI;

public interface IModelGateway
{
    Task<AiResponse<T>> CompleteJsonAsync<T>(
        string taskName,
        object input,
        CancellationToken cancellationToken);

    Task<float[]> CreateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken);
}

public sealed record AiResponse<T>(T Value, string Model, int EstimatedTokens);
