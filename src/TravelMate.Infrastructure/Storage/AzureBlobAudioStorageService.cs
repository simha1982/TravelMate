using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using TravelMate.Application;

namespace TravelMate.Infrastructure.Storage;

public sealed class AzureBlobAudioStorageService(AudioStorageOptions options) : IAudioStorageService
{
    public async Task<StoredAudio> SaveStoryAudioAsync(
        Guid storyId,
        byte[] content,
        string contentType,
        string fileExtension,
        CancellationToken cancellationToken)
    {
        var container = new BlobContainerClient(options.ConnectionString, options.ContainerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

        var safeExtension = string.IsNullOrWhiteSpace(fileExtension) ? "bin" : fileExtension.TrimStart('.');
        var blobName = $"{storyId:N}/{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.{safeExtension}";
        var blob = container.GetBlobClient(blobName);

        await using var stream = new MemoryStream(content);
        await blob.UploadAsync(
            stream,
            new BlobHttpHeaders { ContentType = contentType },
            cancellationToken: cancellationToken);

        var playbackUrl = blob.Uri.ToString();
        if (blob.CanGenerateSasUri)
        {
            var expiresOn = DateTimeOffset.UtcNow.AddHours(Math.Max(1, options.SasHours));
            playbackUrl = blob.GenerateSasUri(BlobSasPermissions.Read, expiresOn).ToString();
        }

        return new StoredAudio(playbackUrl, contentType, content.LongLength);
    }
}
