using Bw.VaultBot.Model;

namespace Bw.VaultBot.Data.Abstractions;

public interface IAdminChatRepository
{
    Task<IReadOnlyList<AdminChat>> GetAdminChats();
    Task<AdminChat?> GetAdminChatById(long id);
    Task AddAdminChat(AdminChat chat);
}