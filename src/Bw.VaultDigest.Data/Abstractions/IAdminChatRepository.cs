using Bw.VaultDigest.Model;

namespace Bw.VaultDigest.Data.Abstractions;

public interface IAdminChatRepository
{
    Task<IReadOnlyList<AdminChat>> GetAdminChats();
    Task<AdminChat?> GetAdminChatById(long id);
    Task AddAdminChat(AdminChat chat);
}