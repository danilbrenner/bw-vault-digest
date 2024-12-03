using System.Diagnostics;
using System.Text.Json;
using Bw.VaultDigest.Common;
using Bw.VaultDigest.Infrastructure.Abstractions;
using Microsoft.Extensions.Logging;

namespace Bw.VaultDigest.Infrastructure.BwClientProvider;

using EnvVariables = IDictionary<string, string>;

public class BwClient(
    ISecretManagerClient secretManagerClient,
    DateTimeProvider dateTimeProvider,
    ILogger<BwClient> logger) : IBwClient
{
    public static readonly JsonSerializerOptions SerializeOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private static EnvVariables EmptyEnvVars => new Dictionary<string, string>();

    public async Task<string> GetUserEmail()
    {
        var status = await Request<StatusInfo>(EmptyEnvVars, "status");
        return
            status switch
            {
                { Status: "unauthenticated" } => await Authenticate(),
                { UserEmail: null } => throw new ApplicationException("Could not get user email"),
                _ => status.UserEmail
            };
    }

    public async Task<IReadOnlyList<Item>> GetItems()
    {
        logger.LogTrace("Getting items");

        var session = await GetSession();

        logger.LogTrace("Session received");

        var items =
            await Request<IReadOnlyList<Item>>(
                EmptyEnvVars.FAdd("BW_SESSION", session),
                "list",
                "items");

        if (items is null)
            throw new Exception("Failed to get logins");

        logger.LogTrace("Got items, {Cnt}", items.Count);

        return items;
    }

    private async Task<string> Unlock()
    {
        logger.LogTrace("Unlocking the vault");

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
        logger.LogTrace("Authenticating vault");

        var apiKeys = await secretManagerClient.GetApiKeys();

        if (apiKeys is null) throw new ApplicationException("Could not retrieve Api Keys to login");

        _ = await Request<string>(
            EmptyEnvVars
                .FAdd("BW_CLIENTID", apiKeys.ClientId)
                .FAdd("BW_CLIENTSECRET", apiKeys.ClientSecret),
            "login",
            "--apikey");

        logger.LogTrace("Vault authentication Succeeded");

        return await GetSession();
    }

    private async Task<string> Sync()
    {
        logger.LogTrace("Synchronizing vault");

        _ = await Request<string>(EmptyEnvVars, "sync");

        logger.LogTrace("Vault synchronization Succeeded");

        return await GetSession();
    }

    private async Task<string> GetSession()
    {
        logger.LogTrace("Getting session");

        var status = await Request<StatusInfo>(EmptyEnvVars, "status");

        logger.LogTrace("Got session status {Status}", status.Status);

        var tmp = dateTimeProvider.UtcNow;

        return
            status switch
            {
                { Status: "unauthenticated" } => await Authenticate(),
                { Status: "locked", LastSync: var lastSync }
                    when lastSync < dateTimeProvider.UtcNow.AddDays(-1) => await Sync(),
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