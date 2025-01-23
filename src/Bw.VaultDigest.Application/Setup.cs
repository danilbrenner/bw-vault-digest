using Bw.VaultDigest.Application.Behaviors;
using Bw.VaultDigest.Application.Options;
using Bw.VaultDigest.Application.Requests;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace Bw.VaultDigest.Application;

public static class Setup
{
    private static string GetTgToken(this IConfiguration configuration)
    {
        var token = configuration.GetSection("Telegram:Token").Get<string>();

        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentNullException(nameof(token), "Telegram bot token is missing");

        return token;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration,
        CancellationToken stoppingToken)
    {
        return services
            .Configure<EmailContentOptions>(configuration.GetSection("EmailDigest:EmailContent"))
            .Configure<AdminOptions>(configuration.GetSection("Admin"))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(RetryBehavior<,>))
            .AddTransient<IPipelineBehavior<TelegramUpdateRequest, Unit>, AuthChatBehaviour>()
            .AddTransient<IPipelineBehavior<TelegramMessageRequest, Unit>, AuthChatBehaviour>()
            .AddSingleton<ITelegramBotClient>(_ =>
                new TelegramBotClient(configuration.GetTgToken(), cancellationToken: stoppingToken))
            .AddMediatR(config => config.RegisterServicesFromAssembly(typeof(Setup).Assembly));
    }

    public static IServiceProvider UseBot(this IServiceProvider services, CancellationToken stoppingToken)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(nameof(Setup));
        var mediator = services.GetRequiredService<IMediator>();
        
        var bot = new TelegramBotClient(configuration.GetTgToken(), cancellationToken: stoppingToken);

        bot.OnError += (exception, _) =>
        {
            logger.LogCritical(exception, "Telegram bot error");
            return Task.CompletedTask;
        };
        
        bot.OnMessage += (message, _) => mediator.Send(new TelegramMessageRequest(message), stoppingToken);
        bot.OnUpdate += update => mediator.Send(new TelegramUpdateRequest(update), stoppingToken);

        var me = bot.GetMe(stoppingToken).GetAwaiter().GetResult();

        logger.LogInformation("Telegram bot({Id}, {Username}) has started", me.Id, me.Username);

        return services;
    }
}