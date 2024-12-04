using Bw.VaultDigest.Model;
using MediatR;

namespace Bw.VaultDigest.Bot.Requests;

public record SyncLoginsCommand : IRequest;
public record SendDigestCommand : IRequest;