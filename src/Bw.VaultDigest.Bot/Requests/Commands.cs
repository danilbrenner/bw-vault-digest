using Bw.VaultDigest.Model;
using MediatR;

namespace Bw.VaultDigest.Bot.Requests;

public record SyncLoginsCommand : IRequest<Unit>;
public record SendDigestCommand : IRequest<Unit>;