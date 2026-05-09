namespace TravelMate.Infrastructure.Storage;

public sealed class AudioStorageOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = "story-audio";
    public string LocalRootPath { get; set; } = "App_Data/story-audio";

    public bool IsAzureConfigured => !string.IsNullOrWhiteSpace(ConnectionString);
}
