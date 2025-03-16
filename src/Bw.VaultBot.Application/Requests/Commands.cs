using MediatR;

namespace Bw.VaultBot.Application.Requests;

public record SyncLoginsCommand : IRequest<Unit>;
public record SendStatisticsCommand : IRequest<Unit>;
public record SendStatisticsToChatCommand(long ChatId) : IRequest<Unit>;