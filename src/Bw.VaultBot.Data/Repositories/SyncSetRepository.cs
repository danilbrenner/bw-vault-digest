using System.Data;
using Bw.VaultBot.Common;
using Bw.VaultBot.Data.Abstractions;
using Bw.VaultBot.Model;
using Dapper;

namespace Bw.VaultBot.Data.Repositories;

public class SyncSetRepository(IDbConnection connection) : ISyncSetRepository
{
    private static Login ToLogin((Guid, string, string?, int, int) loginParts)
    {
        var (loginId, name, userName, strength, age) = loginParts;
        return new Login(loginId, name, userName, age.ToEnum<Age>(), strength.ToEnum<Strength>());
    }

    private LoginsSet ToLoginsSet((Guid, string, DateTime) setParts)
    {
        var (setId, setName, age) = setParts;
        return new LoginsSet(setId, setName, age, []);
    }

    public async Task<LoginsSet?> GetLatestSyncSet()
    {
        LoginsSet? set = null;

        _ = await connection.QueryAsync<
            (Guid, string, DateTime),
            (Guid, string, string?, int, int),
            LoginsSet>(
            """
                with events as (
                    select sync_id, email, timestamp
                    from sync_events
                    order by timestamp desc limit 1
                )
                select e.sync_id, e.email, e.timestamp, l.login_id, l.name, null as user_name, l.strength, l.age
                from logins l
                inner join events e where e.sync_id = l.sync_id;
            """,
            (s, l) =>
            {
                set =
                    s switch
                    {
                        _ when set is null => ToLoginsSet(s) with { Logins = [ToLogin(l)] },
                        var (setId, _, _) when setId == set.Id => set with { Logins = set.Logins.Add(ToLogin(l)) },
                        _ => set
                    };

                return set;
            },
            splitOn: "login_id");

        return set;
    }

    public async Task AddSyncSet(LoginsSet syncSet)
    {
        using var transaction = connection.BeginTransaction();

        await connection
            .ExecuteAsync(
                "INSERT INTO sync_events (sync_id, email, timestamp) VALUES (@Id, @UserEmail, @Timestamp);",
                new { syncSet.Id, syncSet.UserEmail, syncSet.Timestamp },
                transaction);

        foreach (var login in syncSet.Logins)
        {
            await connection
                .ExecuteAsync(
                    "insert into logins(login_id, sync_id, name, user_name, strength, age) values (@Id, @SyncId, @Name, @UserName, @Strength, @Age);",
                    new { SyncId = syncSet.Id, login.Id, login.Name, login.UserName, login.Strength, login.Age },
                    transaction);
        }

        transaction.Commit();
    }
}