using System.Diagnostics;
using System.Text.Json;
using Bw.VaultDigest.Common;
using Microsoft.Extensions.Logging;

namespace Bw.VaultDigest.Infrastructure;

using EnvVariables = IDictionary<string, string>;

public interface IBwClient
{
    public Task<IEnumerable<Item>> GetLogins();
}

public class BwClient(ISecretManagerClient secretManagerClient, ILogger<BwClient> logger) : IBwClient
{
    public static readonly JsonSerializerOptions SerializeOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private static EnvVariables EmptyEnvVars => new Dictionary<string, string>();

    public async Task<IEnumerable<Item>> GetLogins()
    {
        logger.LogInformation("Getting logins");

        var session = await GetSession();

        var items =
            await Request<IEnumerable<Item>>(
                EmptyEnvVars.FAdd("BW_SESSION", session),
                "list",
                "items");

        if (items is null)
            throw new Exception("Failed to get logins");

        return items;
    }

    private async Task<string> Unlock()
    {
        logger.LogInformation("Bw Unlocking");
        var password = await secretManagerClient.GetPassword();
        if (password is null) throw new ApplicationException("Could not retrieve master password");

        return await Request<string>(
            EmptyEnvVars.FAdd("BW_PASSWORD", password),
            "unlock",
            "--passwordenv",
            "BW_PASSWORD", "--raw");
    }

    private async Task<string> Authenticate()
    {
        logger.LogInformation("Bw Authentication Started");

        var apiKeys = await secretManagerClient.GetApiKeys();

        if (apiKeys is null) throw new ApplicationException("Could not retrieve Api Keys to login");

        _ = await Request<string>(
            EmptyEnvVars
                .FAdd("BW_CLIENTID", apiKeys.ClientId)
                .FAdd("BW_CLIENTSECRET", apiKeys.ClientSecret),
            "login",
            "--apikey");

        logger.LogInformation("Bw Authentication Succeeded");

        return await GetSession();
    }

    private async Task<string> GetSession()
    {
        var status = await Request<StatusInfo>(EmptyEnvVars, "status");

        return
            status switch
            {
                { Status: "unauthenticated" } => await Authenticate(),
                { Status: "locked" } => await Unlock(),
                _ => throw new ApplicationException("Unknown status received")
            };
    }

    protected virtual async Task<T> Request<T>(EnvVariables envVariables, string command, params string[] options)
    {
        using var process = new Process();
        process.StartInfo.FileName = "bw";
        process.StartInfo.Arguments =
            options.Aggregate(command, (acc, option) => acc + $" {option}");

        foreach (var (key, value) in envVariables) process.StartInfo.EnvironmentVariables[key] = value;

        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;

        process.Start();

        var outputAwaiter = process.StandardOutput.ReadToEndAsync();
        var errOutAwaiter = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            var errOut = await errOutAwaiter;
            throw new InvalidOperationException(errOut);
        }

        var output = await outputAwaiter;
        if (typeof(T) == typeof(string))
            return (T)(object)output;

        return JsonSerializer.Deserialize<T>(output, SerializeOptions)
               ?? throw new InvalidOperationException("Failed to serialize status info");
    }
}