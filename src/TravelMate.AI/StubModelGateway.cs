namespace TravelMate.AI;

public sealed class StubModelGateway : IModelGateway
{
    public Task<AiResponse<T>> CompleteJsonAsync<T>(
        string taskName,
        object input,
        CancellationToken cancellationToken)
    {
        var value = Activator.CreateInstance<T>();
        return Task.FromResult(new AiResponse<T>(value!, "stub-local", 0));
    }

    public Task<float[]> CreateEmbeddingAsync(string text, CancellationToken cancellationToken)
    {
        var vector = new float[16];
        for (var index = 0; index < vector.Length && index < text.Length; index++)
        {
            vector[index] = char.ToLowerInvariant(text[index]) / 255f;
        }

        return Task.FromResult(vector);
    }
}
