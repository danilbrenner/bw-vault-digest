using Bw.VaultBot.Application.Requests;
using Bw.VaultBot.Common;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Bw.VaultBot.Application.Handlers;

public class ReceivedRequestHandler(ITelegramBotClient bot, IMediator mediator)
    : IRequestHandler<TelegramMessageRequest, Unit>
{
    public Task<Unit> Handle(TelegramMessageRequest request, CancellationToken cancellationToken)
    {
        return request.Message.Type switch
        {
            MessageType.Text when request.Message.Text == "/start" =>
                bot
                    .SendMessage(request.Message.Chat, "Hey there", cancellationToken: cancellationToken)
                    .ToUnit(),
            MessageType.Text when request.Message.Text == "/statistics" =>
                mediator
                    .Send(new SendStatisticsToChatCommand(request.Message.Chat.Id), cancellationToken)
                    .ToUnit(),
            MessageType.Text when request.Message.Text == "/sync" =>
                mediator
                    .Send(new SyncLoginsCommand(), cancellationToken)
                    .ToUnit(),
            _ => Task.FromResult(Unit.Value)
        };
    }
}