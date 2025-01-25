using MediatR;

namespace Bw.VaultDigest.Application.Requests;

public record SyncLoginsCommand : IRequest<Unit>;
public record SendDigestCommand : IRequest<Unit>;
public record SendDigestToChatCommand(long ChatId) : IRequest<Unit>;