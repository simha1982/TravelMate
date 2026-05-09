using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TravelMate.AI;

public sealed class AzureOpenAiModelGateway(HttpClient httpClient, AzureOpenAiOptions options) : IModelGateway
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<AiResponse<T>> CompleteJsonAsync<T>(
        string taskName,
        object input,
        CancellationToken cancellationToken)
    {
        EnsureConfigured();

        var uri = $"{options.Endpoint.TrimEnd('/')}/openai/deployments/{options.ChatDeployment}/chat/completions?api-version={options.ApiVersion}";
        using var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Headers.Add("api-key", options.ApiKey);
        request.Content = JsonContent.Create(new
        {
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = "You are TravelMate's AI service. Return only valid compact JSON matching the requested shape. Ground travel content in supplied source text and avoid unsupported facts."
                },
                new
                {
                    role = "user",
                    content = JsonSerializer.Serialize(new { taskName, input }, JsonOptions)
                }
            },
            temperature = 0.2,
            response_format = new { type = "json_object" }
        }, options: JsonOptions);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<JsonObject>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Azure OpenAI returned an empty response.");

        var content = payload["choices"]?[0]?["message"]?["content"]?.GetValue<string>()
            ?? throw new InvalidOperationException("Azure OpenAI response did not include message content.");

        var value = JsonSerializer.Deserialize<T>(content, JsonOptions)
            ?? throw new InvalidOperationException("Azure OpenAI returned JSON that could not be deserialized.");

        var tokens = payload["usage"]?["total_tokens"]?.GetValue<int>() ?? 0;
        return new AiResponse<T>(value, options.ChatDeployment, tokens);
    }

    public async Task<float[]> CreateEmbeddingAsync(string text, CancellationToken cancellationToken)
    {
        EnsureConfigured();
        if (string.IsNullOrWhiteSpace(options.EmbeddingDeployment))
        {
            throw new InvalidOperationException("AzureOpenAI:EmbeddingDeployment is required for embeddings.");
        }

        var uri = $"{options.Endpoint.TrimEnd('/')}/openai/deployments/{options.EmbeddingDeployment}/embeddings?api-version={options.ApiVersion}";
        using var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Headers.Add("api-key", options.ApiKey);
        request.Content = JsonContent.Create(new { input = text }, options: JsonOptions);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<JsonObject>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Azure OpenAI returned an empty embedding response.");

        return payload["data"]?[0]?["embedding"]?.Deserialize<float[]>(JsonOptions)
            ?? throw new InvalidOperationException("Azure OpenAI embedding response did not include a vector.");
    }

    private void EnsureConfigured()
    {
        if (!options.IsConfigured)
        {
            throw new InvalidOperationException("Azure OpenAI is not configured. Set AzureOpenAI:Endpoint, ApiKey, and ChatDeployment.");
        }
    }
}
