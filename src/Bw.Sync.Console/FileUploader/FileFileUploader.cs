using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bw.Sync.Console.FileUploader;

public class FileFileUploader(IConfiguration configuration, ILogger<FileFileUploader> logger) : IFileUploader
{
    public async Task Upload(string path, Stream data, CancellationToken cancellationToken)
    {
        logger.LogInformation("Saving file using local file system");
        
        var basePath = configuration["UploadTo:BasePath"] ?? string.Empty;
        var fullPath = Path.GetFullPath(Path.Combine(basePath, path));
        var directory = Path.GetDirectoryName(fullPath);
        if (directory is not null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        data.Position = 0;
        await using var fileStream = File.Create(fullPath);
        await data.CopyToAsync(fileStream, cancellationToken);
        
        logger.LogInformation("File saved to {Path}", fullPath);
    }
}

