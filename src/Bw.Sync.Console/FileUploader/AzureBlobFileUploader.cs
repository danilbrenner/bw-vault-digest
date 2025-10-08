using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bw.Sync.Console.FileUploader;

public class AzureBlobFileUploader(IConfiguration configuration, ILogger<AzureBlobFileUploader> logger) : IFileUploader
{
    public async Task Upload(string path, Stream data, CancellationToken cancellationToken)
    {
        logger.LogInformation("Uploading file using Azure Blob Storage");
        
        var connectionString = configuration["UploadTo:ConnectionString"] ?? string.Empty;
        var containerName = configuration["UploadTo:ContainerName"] ?? string.Empty;
        
        var blobClient = new BlobContainerClient(connectionString, containerName);
        await blobClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        var blob = blobClient.GetBlobClient(path);
        data.Position = 0;
        await blob.UploadAsync(data, overwrite: true, cancellationToken);
        
        logger.LogInformation("File uploaded to {Container}::{Path}", containerName, path);
    }
}

