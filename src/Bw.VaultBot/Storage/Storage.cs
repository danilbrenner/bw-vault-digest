using Azure.Storage.Blobs;

namespace Bw.VaultBot.Storage;

public interface IStorage
{
    Task SaveAsync(string path, Stream data, CancellationToken cancellationToken);
}

public class FileStorage(string basePath) : IStorage
{
    public async Task SaveAsync(string path, Stream data, CancellationToken cancellationToken)
    {
        var fullPath = Path.Combine(basePath, path);
        var directory = Path.GetDirectoryName(fullPath);
        if (directory is not null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        data.Position = 0;
        await using var fileStream = File.Create(fullPath);
        await data.CopyToAsync(fileStream, cancellationToken);
    }
}

public class AzureBlobStorage(string connectionString, string containerName) : IStorage
{
    private readonly BlobContainerClient _blobClient = new(connectionString, containerName);

    public async Task SaveAsync(string path, Stream data, CancellationToken cancellationToken)
    {
        await _blobClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        var blob = _blobClient.GetBlobClient(path);
        data.Position = 0;
        await blob.UploadAsync(data, overwrite: true, cancellationToken);
    }
}