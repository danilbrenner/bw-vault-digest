using Bw.VaultDigest.Application.Requests;
using Bw.VaultDigest.Data.Abstractions;
using Bw.VaultDigest.Infrastructure.EmailNotifierClient;
using Bw.VaultDigest.Model;
using Bw.VaultDigest.Telemetry;
using MediatR;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bw.VaultDigest.Application.Handlers;

public class SendDigestHandler(
    MetricsFactory metricsFactory,
    ILogger<SendDigestHandler> logger,
    ITelegramBotClient bot,
    IAdminChatRepository adminChatRepository,
    ISyncSetRepository repository)
    : IRequestHandler<SendDigestCommand, Unit>
        , IRequestHandler<SendDigestToChatCommand, Unit>
{
    private async Task SendDigest(LoginsSet set, IEnumerable<long> chatIds, CancellationToken cancellationToken)
    {
        var age = set.Logins.ToAgeSlices().ToDoughnutDiagram();
        var strength = set.Logins.ToStrengthSlices().ToDoughnutDiagram();

        foreach (var chatId in chatIds)
        {
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

    public async Task<Unit> Handle(SendDigestCommand command, CancellationToken cancellationToken)
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
        await SendDigest(set, chats.Select(x => x.ChatId), cancellationToken);

        logger.LogInformation("Digest sent to {ChatCount} chats", chats.Count);
        return Unit.Value;
    }

    public async Task<Unit> Handle(SendDigestToChatCommand request, CancellationToken cancellationToken)
    {
        var set = await repository.GetLatestSyncSet();

        if (set is null)
        {
            logger.LogError("No synchronization set was found to create the digest email");
            return Unit.Value;
        }

        await SendDigest(set, [request.ChatId], cancellationToken);

        return Unit.Value;
    }
}