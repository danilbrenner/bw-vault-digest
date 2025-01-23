using Bw.VaultDigest.Application.Requests;
using Bw.VaultDigest.Data.Abstractions;
using Bw.VaultDigest.Infrastructure.EmailNotifierClient;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bw.VaultDigest.Application.Handlers;

public class TelegramRequestHandler(ITelegramBotClient bot, ISyncSetRepository repository)
    : IRequestHandler<TelegramMessageRequest, Unit>
{
    private async Task<Unit> SendMessage(Chat chat, string text, CancellationToken cancellationToken)
    {
        await bot.SendMessage(chat, text, cancellationToken: cancellationToken);
        return Unit.Value;
    }

    private async Task<Unit> SendStatistics(Chat chat, string text, CancellationToken cancellationToken)
    {
        var set = await repository.GetLatestSyncSet();
        if (set == null)
            return await SendMessage(chat, "Sorry. No synchronization yet", cancellationToken: cancellationToken);

        var age = set.Logins.ToAgeSlices().ToDoughnutDiagram();
        var strength = set.Logins.ToStrengthSlices().ToDoughnutDiagram();

        await bot.SendMessage(chat, "Strength", cancellationToken: cancellationToken);
        await bot.SendSticker(
            chat, InputFile.FromStream(new MemoryStream(strength)), cancellationToken: cancellationToken);
        await bot.SendMessage(chat, "Age", cancellationToken: cancellationToken);
        await bot.SendSticker(
            chat, InputFile.FromStream(new MemoryStream(age)), cancellationToken: cancellationToken);

        return Unit.Value;
    }

    public Task<Unit> Handle(TelegramMessageRequest request, CancellationToken cancellationToken)
    {
        return request.Message.Type switch
        {
            MessageType.Text when request.Message.Text == "/start" =>
                SendMessage(request.Message.Chat, "Hey there", cancellationToken),
            MessageType.Text when request.Message.Text == "/statistics" =>
                SendStatistics(request.Message.Chat, "Stats", cancellationToken),
            _ => Task.FromResult(Unit.Value)
        };
    }
}