namespace TravelMate.Application;

public interface IAudioStorageService
{
    Task<StoredAudio> SaveStoryAudioAsync(
        Guid storyId,
        byte[] content,
        string contentType,
        string fileExtension,
        CancellationToken cancellationToken);
}

public sealed record StoredAudio(string Url, string ContentType, long Length);
