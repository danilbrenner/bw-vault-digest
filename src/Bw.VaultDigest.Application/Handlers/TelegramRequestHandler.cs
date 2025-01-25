using Bw.VaultDigest.Application.Requests;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bw.VaultDigest.Application.Handlers;

public class TelegramRequestHandler(ITelegramBotClient bot, IMediator mediator)
    : IRequestHandler<TelegramMessageRequest, Unit>
{
    private async Task<Unit> SendMessage(Chat chat, string text, CancellationToken cancellationToken)
    {
        await bot.SendMessage(chat, text, cancellationToken: cancellationToken);
        return Unit.Value;
    }

    public Task<Unit> Handle(TelegramMessageRequest request, CancellationToken cancellationToken)
    {
        return request.Message.Type switch
        {
            MessageType.Text when request.Message.Text == "/start" =>
                SendMessage(request.Message.Chat, "Hey there", cancellationToken),
            MessageType.Text when request.Message.Text == "/statistics" =>
                mediator.Send(new SendDigestToChatCommand(request.Message.Chat.Id), cancellationToken),
            _ => Task.FromResult(Unit.Value)
        };
    }
}