using System.Data;
using System.Reflection;
using Bw.VaultBot.Data.Abstractions;
using Bw.VaultBot.Data.Repositories;
using Dapper;
using DbUp;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bw.VaultBot.Data;

public static class Setup
{
    
    public static IServiceCollection AddData(this IServiceCollection services, IConfiguration cfg)
    {
        SqlMapper.AddTypeHandler(new DateTimeHandler());
        SqlMapper.AddTypeHandler(new GuidTypeHandler());
        
        return services
            .Configure<DataOptions>(cfg.GetSection("Data"))
            .AddScoped<IDbConnection>(sp =>
            {
                var connection =
                    new SqliteConnection(sp.GetRequiredService<IOptions<DataOptions>>().Value.ConnectionString);
                connection.Open();
                return connection;
            })
            .AddTransient<ISyncSetRepository, SyncSetRepository>()
            .AddTransient<IAdminChatRepository, AdminChatRepository>();
    }

    public static IServiceProvider ApplyMigrations(this IServiceProvider sp)
    {
        var options = sp.GetRequiredService<IOptions<DataOptions>>().Value;
        
        var upgrader =
            DeployChanges.To.SQLiteDatabase(options.ConnectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .LogToAutodetectedLog()
                .Build();
        
        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            throw result.Error;
        }
        
        return sp;
    }
}