using System.Globalization;
using System.Text;
using Bw.VaultBot.Model;
using CsvHelper;

namespace Bw.VaultBot.Jobs;

public class ExportLoginsJob(ILogger<ExportLoginsJob> logger)
{
    public async Task Execute()
    {
        await RunAsync(CancellationToken.None);
    }

    private static async Task<MemoryStream> ToCsv(LoginsSet set)
    {
        var memoryStream = new MemoryStream();

        await using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true);
        await using var csv = new CsvWriter(streamWriter, CultureInfo.InvariantCulture, true);

        await csv.WriteRecordsAsync(set.Logins);
        await streamWriter.FlushAsync();

        return memoryStream;
    }

    // private static async Task<Unit> SaveToFile(MemoryStream stream)
    // {
    //     stream.Position = 0;
    //     await using var file = File.Create("output.csv");
    //     await stream.CopyToAsync(file);
    //     return Unit.Default;
    // }
    //
    // private static Func<MemoryStream, Task<Unit>> UploadToAzure(string connectionString, string containerName, string blobName) =>
    //     async stream =>
    //     {
    //         stream.Position = 0;
    //
    //         var blobClient = new BlobContainerClient(connectionString, containerName);
    //         await blobClient.CreateIfNotExistsAsync();
    //
    //         var blob = blobClient.GetBlobClient(blobName);
    //         await blob.UploadAsync(stream, overwrite: true);
    //         return Unit.Default;
    //     };

    protected async Task RunAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting sync service");
        await Task.Delay(30000, cancellationToken);
        
        // var logins = await
        //     TryAsync(loginProvider.GetLogins)
        //         .ThenTry(ToCsv)
        //         .ThenTry(SaveToFile)
        //         // .ThenTry(UploadToAzure("", "tests", "logins.csv"))
        //         .Match(
        //             set => $"All done!!",
        //             err => err.Message);
    }
}