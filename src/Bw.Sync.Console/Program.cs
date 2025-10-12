using Bw.Sync.BwCliClient;
using Bw.Sync.BwCliClient.Abstractions;
using Bw.Sync.BwCliClient.BwClientProvider;
using Bw.Sync.BwCliClient.Models;
using Bw.Sync.Console.FileUploader;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Bw.Sync.Console;

using System;
using Microsoft.Extensions.DependencyInjection;

public static class Program
{
    private static string ReadMasterPassword()
    {
        Console.Write("Enter master password: ");
        return Console.ReadLine() ?? string.Empty;
    }

    public static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .CreateLogger();

        try
        {
            Log.Information("Init Application");

            using var host = Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddSerilog();
                })
                .ConfigureServices((context, services) =>
                {
                    services
                        .AddInfrastructure()
                        .AddFileUploader(context.Configuration);
                })
                .Build();


            Log.Information("Application started");

            var provider = host.Services.GetRequiredService<ILoginProviderAdapter>();
            var fileUploader = host.Services.GetRequiredService<IFileUploader>();

            var clientId = configuration["ApiKeys:ClientId"] ?? string.Empty;
            var clientSecret = configuration["ApiKeys:ClientSecret"] ?? string.Empty;
            var hashSalt = configuration["HashSalt"] ?? string.Empty;

            var credentials = new SecurityCredentials(
                ReadMasterPassword(),
                new ApiKeys(clientId, clientSecret));

            var logins = await provider.GetLogins(credentials);
            using var loginsStream = await logins.ToCsvStream();

            var folder = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            
            await fileUploader.Upload($"{folder}/logins.csv", loginsStream, CancellationToken.None);

            await using var mpStream = credentials.GetPasswordHash(hashSalt).ToStream();
            await fileUploader.Upload($"{folder}/mp", mpStream, CancellationToken.None);

            Log.Information("Application finished successfully");
            Environment.ExitCode = 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            Environment.ExitCode = 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}