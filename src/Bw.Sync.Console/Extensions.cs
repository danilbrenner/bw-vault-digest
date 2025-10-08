using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Bw.Sync.BwCliClient.Models;
using CsvHelper;

namespace Bw.Sync.Console;

public static class Extensions
{
    public static async Task<MemoryStream> ToCsvStream(this LoginsSet set)
    {
        var memoryStream = new MemoryStream();
        await using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true);
        await using var csv = new CsvWriter(streamWriter, CultureInfo.InvariantCulture, true);
        await csv.WriteRecordsAsync(set.Logins);
        await streamWriter.FlushAsync();
        return memoryStream;
    }

    public static string ToCsvFileName(this DateTime dateTime)
        => $"logins_{dateTime:yyyyMMddHHmmss}.csv";
}

