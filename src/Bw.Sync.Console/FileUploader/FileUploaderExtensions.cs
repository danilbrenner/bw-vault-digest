using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bw.Sync.Console.FileUploader;

public static class FileUploaderExtensions
{
    public static IServiceCollection AddFileUploader(this IServiceCollection services, IConfiguration configuration)
    {
        var uploadTo = configuration["UploadTo:Type"] ?? string.Empty;
        switch (uploadTo)
        {
            case "AzureBlob":
                services.AddTransient<IFileUploader, AzureBlobFileUploader>();
                break;
            case "File":
                services.AddTransient<IFileUploader, FileFileUploader>();
                break;
            default:
                throw new ApplicationException("Unknown storage type");
        }

        return services;
    }
}

