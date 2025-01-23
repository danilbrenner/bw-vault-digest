using Bw.VaultDigest.Application.Options;
using Bw.VaultDigest.Application.Requests;
using Bw.VaultDigest.Data.Abstractions;
using Bw.VaultDigest.Model;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bw.VaultDigest.Application.Behaviors;

public class AuthChatBehaviour(
    IOptions<AdminOptions> options,
    IAdminChatRepository repository,
    ITelegramBotClient bot,
    ILogger<AuthChatBehaviour> logger)
    : IPipelineBehavior<TelegramMessageRequest, Unit>
        , IPipelineBehavior<TelegramUpdateRequest, Unit>
{
    private async Task<Unit> RequestIntroduction(Chat chat, CancellationToken cancellationToken)
    {
        await bot.SendMessage(
            chat,
            "I don't know you. Introduce yourself"
            , replyMarkup: new ReplyKeyboardMarkup()
                .AddButton(new KeyboardButton("Share Contact") { RequestContact = true }),
            cancellationToken: cancellationToken);
        return Unit.Value;
    }

    private async Task<Unit> CreateAdminChat(Chat chat, string username, string phoneNr, CancellationToken cancellationToken)
    {
        await repository.AddAdminChat(new AdminChat(chat.Id, username, phoneNr));
        await bot.SendMessage(
            chat, 
            "Welcome to the Digest!!!", 
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);
        return Unit.Value;
    }

    public async Task<Unit> Handle(TelegramMessageRequest request, RequestHandlerDelegate<Unit> next,
        CancellationToken cancellationToken)
    {
        var chat = await repository.GetAdminChatById(request.Message.Chat.Id);
        return
            (chat is null, request.Message.Type) switch
            {
                (false, _) =>
                    await next(),
                (true, MessageType.Contact)
                    when request.Message.Chat.Id == request.Message.Contact?.UserId
                         && options.Value.PhoneNr == request.Message.Contact?.PhoneNumber
                         && options.Value.Username == request.Message.Chat.Username =>
                    await CreateAdminChat(
                        request.Message.Chat, 
                        request.Message.Chat.Username,
                        request.Message.Contact?.PhoneNumber ?? options.Value.PhoneNr, 
                        cancellationToken),
                (true, _) =>
                    await RequestIntroduction(request.Message.Chat, cancellationToken)
            };
    }

    public Task<Unit> Handle(TelegramUpdateRequest request, RequestHandlerDelegate<Unit> next,
        CancellationToken cancellationToken)
    {
        return Unit.Task;
    }
}