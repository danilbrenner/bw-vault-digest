using Bw.VaultDigest.Model;
using MediatR;

namespace Bw.VaultDigest.Web.Requests;

public record SyncLoginsCommand : IRequest;