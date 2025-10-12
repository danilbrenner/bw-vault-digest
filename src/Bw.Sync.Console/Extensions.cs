using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Bw.Sync.BwCliClient.Abstractions;
using Bw.Sync.BwCliClient.Models;
using CsvHelper;

namespace Bw.Sync.Console;

public static class Extensions
{
    public static Stream ToStream(this string value)
        => new MemoryStream(Encoding.UTF8.GetBytes(value));

    public static async Task<MemoryStream> ToCsvStream(this LoginsSet set)
    {
        var memoryStream = new MemoryStream();
        await using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true);
        await using var csv = new CsvWriter(streamWriter, CultureInfo.InvariantCulture, true);
        await csv.WriteRecordsAsync(set.Logins);
        await streamWriter.FlushAsync();
        return memoryStream;
    }

    public static string GetPasswordHash(this ICredentials credentials, string salt)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{credentials.Password}{salt}"));
        return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLowerInvariant();
    }
}