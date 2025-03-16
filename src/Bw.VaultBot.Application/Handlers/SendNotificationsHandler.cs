using Bw.VaultBot.Application.Requests;
using Bw.VaultBot.Common;
using Bw.VaultBot.Data.Abstractions;
using Bw.VaultBot.Infrastructure.EmailNotifierClient;
using Bw.VaultBot.Model;
using Bw.VaultBot.Telemetry;
using MediatR;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bw.VaultBot.Application.Handlers;

public class SendNotificationsHandler(
    MetricsFactory metricsFactory,
    ILogger<SendNotificationsHandler> logger,
    ITelegramBotClient bot,
    IAdminChatRepository adminChatRepository,
    ISyncSetRepository repository)
        : IRequestHandler<SendStatisticsCommand, Unit>
        , IRequestHandler<SendStatisticsToChatCommand, Unit>
{
    
    
    private async Task SendStatistics(LoginsSet set, IEnumerable<long> chatIds, CancellationToken cancellationToken)
    {
        var age = set.Logins.ToAgeSlices().ToDoughnutDiagram();
        var strength = set.Logins.ToStrengthSlices().ToDoughnutDiagram();

        foreach (var chatId in chatIds)
        {
            await bot.SendMessage(
                chatId, $"You have: {set.Logins.Count} logins in your vault.", cancellationToken: cancellationToken);
            await bot.SendMessage(
                chatId, "Strength", cancellationToken: cancellationToken);
            await bot.SendSticker(
                chatId, InputFile.FromStream(new MemoryStream(strength)), cancellationToken: cancellationToken);
            await bot.SendMessage(
                chatId, "Age", cancellationToken: cancellationToken);
            await bot.SendSticker(
                chatId, InputFile.FromStream(new MemoryStream(age)), cancellationToken: cancellationToken);
        }
    }

    public async Task<Unit> Handle(SendStatisticsCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Sending digest");
        using var _ = metricsFactory.CreateDurationMetric("send-digest.duration");

        var set = await repository.GetLatestSyncSet();

        if (set is null)
        {
            logger.LogError("No synchronization set was found to create the digest email");
            return Unit.Value;
        }

        var chats = await adminChatRepository.GetAdminChats();
        await SendStatistics(set, chats.Select(x => x.ChatId), cancellationToken);

        logger.LogInformation("Digest sent to {ChatCount} chats", chats.Count);
        return Unit.Value;
    }

    public async Task<Unit> Handle(SendStatisticsToChatCommand request, CancellationToken cancellationToken)
    {
        var set = await repository.GetLatestSyncSet();

        if (set is not null)
            return await SendStatistics(set, [request.ChatId], cancellationToken).ToUnit();
        
        logger.LogError("No synchronization set was found to create the digest email");
        await bot.SendMessage(
            request.ChatId, "Logins were not synchronized. Please synchronize and try again.", cancellationToken: cancellationToken);
        return Unit.Value;
    }
}