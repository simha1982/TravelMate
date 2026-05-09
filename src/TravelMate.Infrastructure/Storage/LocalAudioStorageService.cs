using TravelMate.Application;

namespace TravelMate.Infrastructure.Storage;

public sealed class LocalAudioStorageService(AudioStorageOptions options) : IAudioStorageService
{
    public async Task<StoredAudio> SaveStoryAudioAsync(
        Guid storyId,
        byte[] content,
        string contentType,
        string fileExtension,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(options.LocalRootPath);
        var safeExtension = string.IsNullOrWhiteSpace(fileExtension) ? "bin" : fileExtension.TrimStart('.');
        var fileName = $"{storyId:N}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.{safeExtension}";
        var path = Path.Combine(options.LocalRootPath, fileName);
        await File.WriteAllBytesAsync(path, content, cancellationToken);
        return new StoredAudio(path, contentType, content.LongLength);
    }
}
