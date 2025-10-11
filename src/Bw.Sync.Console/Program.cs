using System.Security.Cryptography;
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
    private static byte[] ReadMasterPassword()
    {
        Console.Write("Enter master password: ");

        var buffer = new byte[256];
        var length = 0;

        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    Console.WriteLine();
                    var password = new byte[length];
                    Buffer.BlockCopy(buffer, 0, password, 0, length);
                    CryptographicOperations.ZeroMemory(buffer); // Zero temp buffer
                    return password;
                case ConsoleKey.Backspace when length > 0:
                    length--;
                    Console.Write("\b \b");
                    continue;
                default:
                {
                    if (char.IsLetterOrDigit(key.KeyChar) || char.IsPunctuation(key.KeyChar) ||
                        char.IsSymbol(key.KeyChar))
                    {
                        buffer[length] = (byte)key.KeyChar;
                        length++;
                        Console.Write("*");
                    }

                    continue;
                }
            }
        }
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


            using (var credentials =
                   new SecureCredentials(
                       ReadMasterPassword(),
                       new ApiKeys(clientId, clientSecret))
                  )
            {
                var logins = await provider.GetLogins(credentials);

                var stream = logins.ToCsvStream();

                var fileName = DateTime.UtcNow.ToCsvFileName();
                await fileUploader.Upload(fileName, await stream, CancellationToken.None);
            }

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