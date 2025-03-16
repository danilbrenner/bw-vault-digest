using MediatR;
using Telegram.Bot.Types;

namespace Bw.VaultBot.Application.Requests;

public record TelegramMessageRequest(Message Message) : IRequest<Unit>;
public record TelegramUpdateRequest(Update Update) : IRequest<Unit>;
