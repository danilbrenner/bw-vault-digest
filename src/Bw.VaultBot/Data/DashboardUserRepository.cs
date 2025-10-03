using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Builder;
using System.IO;

namespace Bw.VaultBot.Data;

public record DashboardUser(string Username, string PasswordHash);

public interface IDashboardUserRepository
{
    Task<DashboardUser?> GetUserByUsername(string username);
    Task UpdateLastLoginTime(string username, DateTime loginTime);
}

public class DashboardUserRepository(string connectionString) : IDashboardUserRepository
{
    public async Task<DashboardUser?> GetUserByUsername(string username)
    {
        await using var connection = new SqlConnection(connectionString);
        return await connection.QuerySingleOrDefaultAsync<DashboardUser>(
            "SELECT Username, PasswordHash FROM HangfireDashboardUsers WHERE Username = @Username",
            new { Username = username });
    }

    public async Task UpdateLastLoginTime(string username, DateTime loginTime)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.ExecuteAsync(
            "UPDATE HangfireDashboardUsers SET LastLoginAt = @LoginTime WHERE Username = @Username",
            new { Username = username, LoginTime = loginTime });
    }
}

public static class DashboardUserRepositoryExtensions
{
    public static IApplicationBuilder MigrateDashboardUsers(this IApplicationBuilder app, string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        var sql = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Data", "InitializeHangfireDashboard.sql"));
        connection.Execute(sql);
        return app;
    }
}
