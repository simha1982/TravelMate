namespace TravelMate.AI;

public sealed class StubModelGateway : IModelGateway
{
    public Task<AiResponse<T>> CompleteJsonAsync<T>(
        string taskName,
        object input,
        CancellationToken cancellationToken)
    {
        var value = CreateStubValue<T>(taskName);
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

    private static T CreateStubValue<T>(string taskName)
    {
        var type = typeof(T);
        if (type.GetConstructor(Type.EmptyTypes) is not null)
        {
            return Activator.CreateInstance<T>();
        }

        var constructor = type.GetConstructors()
            .OrderByDescending(item => item.GetParameters().Length)
            .FirstOrDefault();

        if (constructor is null)
        {
            throw new InvalidOperationException($"Cannot create stub AI response for {type.Name}.");
        }

        var arguments = constructor.GetParameters()
            .Select(parameter => CreateDefaultValue(parameter.ParameterType, parameter.Name ?? string.Empty, taskName))
            .ToArray();

        return (T)constructor.Invoke(arguments);
    }

    private static object? CreateDefaultValue(Type type, string parameterName, string taskName)
    {
        if (type == typeof(string))
        {
            return parameterName.Contains("title", StringComparison.OrdinalIgnoreCase)
                ? "TravelMate local summary"
                : $"Local stub response for {taskName}.";
        }

        if (type == typeof(bool))
        {
            return true;
        }

        if (type == typeof(int))
        {
            return 0;
        }

        if (type == typeof(string[]))
        {
            return Array.Empty<string>();
        }

        if (type.IsArray)
        {
            return Array.CreateInstance(type.GetElementType() ?? typeof(object), 0);
        }

        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }
}
