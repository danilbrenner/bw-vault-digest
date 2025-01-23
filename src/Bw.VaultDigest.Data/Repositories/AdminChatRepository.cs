using System.Data;
using Bw.VaultDigest.Data.Abstractions;
using Bw.VaultDigest.Model;
using Dapper;

namespace Bw.VaultDigest.Data.Repositories;

public class AdminChatRepository(IDbConnection connection) : IAdminChatRepository
{
    public async Task<AdminChat?> GetAdminChatById(long id)
    {
        return await connection.QueryFirstOrDefaultAsync<AdminChat>(
            """
                select chat_id as ChatId, username as Username, phone_nr as PhoneNr
                from admin_chats where chat_id = @id;
            """,
            new { id });
    }

    public async Task AddAdminChat(AdminChat chat)
    {
        await connection.ExecuteAsync(
            "insert into admin_chats(chat_id, username, phone_nr) values (@ChatId, @Username, @PhoneNr)", chat);
    }
}