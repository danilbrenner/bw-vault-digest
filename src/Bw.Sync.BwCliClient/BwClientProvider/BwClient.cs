using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Bw.Sync.BwCliClient.Abstractions;
using Bw.Sync.BwCliClient.Models;
using Microsoft.Extensions.Logging;

namespace Bw.Sync.BwCliClient.BwClientProvider;

using EnvVariables = IDictionary<string, string>;

public class BwClient(
    DateTimeProvider dateTimeProvider,
    ILogger<BwClient> logger) : IBwClient
{
    public static readonly JsonSerializerOptions SerializeOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private static EnvVariables EmptyEnvVars => new Dictionary<string, string>();

    public async Task<string> GetUserEmail(ICredentials credentials)
    {
        var status = await Request<StatusInfo>(EmptyEnvVars, "status");
        return
            status switch
            {
                { Status: "unauthenticated" } => await Authenticate(credentials),
                { UserEmail: null } => throw new ApplicationException("Could not get user email"),
                _ => status.UserEmail
            };
    }

    public async Task<IReadOnlyList<Item>> GetItems(ICredentials credentials)
    {
        logger.LogInformation("Getting items");

        var session = await GetSession(credentials);

        logger.LogInformation("Session received");

        var items =
            await Request<IReadOnlyList<Item>>(
                EmptyEnvVars.FAdd("BW_SESSION", session),
                "list",
                "items");

        if (items is null)
            throw new Exception("Failed to get logins");

        logger.LogInformation("Got items, {Cnt}", items.Count);

        return items;
    }

    private async Task<string> Unlock(ICredentials credentials)
    {
        logger.LogInformation("Unlocking the vault");

        // Necassary evil: bw cli accepts only string, Should refactor to use string everywhere(ok for short lived process)
        var password = Encoding.UTF8.GetString(credentials.Password.Span); 
        
        var session = await Request<string>(
            EmptyEnvVars.FAdd("BW_PASSWORD", password),
            "unlock",
            "--passwordenv", "BW_PASSWORD", 
            "--raw");

        return session;
    }

    private async Task<string> Authenticate(ICredentials credentials)
    {
        logger.LogInformation("Authenticating vault");
        
        _ = await Request<string>(
            EmptyEnvVars
                .FAdd("BW_CLIENTID", credentials.ApiKeys.ClientId)
                .FAdd("BW_CLIENTSECRET", credentials.ApiKeys.ClientSecret),
            "login",
            "--apikey");

        logger.LogInformation("Vault authentication Succeeded");

        return await GetSession(credentials);
    }

    private async Task<string> Sync(ICredentials credentials)
    {
        logger.LogInformation("Synchronizing vault");

        _ = await Request<string>(EmptyEnvVars, "sync");

        logger.LogInformation("Vault synchronization Succeeded");

        return await GetSession(credentials);
    }

    private async Task<string> GetSession(ICredentials credentials)
    {
        logger.LogInformation("Getting session");

        var status = await Request<StatusInfo>(EmptyEnvVars, "status");

        logger.LogInformation("Got session status {Status}", status.Status);

        return
            status switch
            {
                { Status: "unauthenticated" } => await Authenticate(credentials),
                { Status: "locked", LastSync: var lastSync }
                    when lastSync < dateTimeProvider.UtcNow.AddDays(-1) => await Sync(credentials),
                { Status: "locked" } => await Unlock(credentials),
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