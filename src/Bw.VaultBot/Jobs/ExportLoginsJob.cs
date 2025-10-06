using System.Globalization;
using System.Text;
using Bw.Sync.BwCliClient.Abstractions;
using Bw.VaultBot.Model;
using Bw.VaultBot.Storage;
using CsvHelper;

namespace Bw.VaultBot.Jobs;

public class ExportLoginsJob(ILogger<ExportLoginsJob> logger, ILoginProviderAdapter loginProvider, IStorage storage)
{
    public async Task Execute(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting sync job");
        
        var logins = await loginProvider.GetLogins();
        await storage.SaveAsync(DateTime.Now.ToCsvFileName(), await ToCsv(logins), cancellationToken);
        
        logger.LogInformation("Sync job completed");

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
}