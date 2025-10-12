using System.Globalization;
using System.Text;
using Bw.Sync.BwCliClient.Abstractions;
using Bw.Sync.BwCliClient.Models;
using CsvHelper;
using Konscious.Security.Cryptography;

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

    public static async Task<string> GetPasswordHash(this ICredentials credentials, string pepper)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(credentials.Password);
        var pepperBytes = Encoding.UTF8.GetBytes(pepper);

        using var argon2 = new Argon2id(passwordBytes);
        argon2.Salt = pepperBytes;
        argon2.DegreeOfParallelism = 1;
        argon2.Iterations = 2;
        argon2.MemorySize = 1024 * 64;
        
        var hashBytes = await argon2.GetBytesAsync(32);
        
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}