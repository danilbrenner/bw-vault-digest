using Hangfire.Dashboard;
using Bw.VaultBot.Data;
using static BCrypt.Net.BCrypt;

namespace Bw.VaultBot.Filters;

public class AuthorizationFilter(
    ILogger<AuthorizationFilter> logger,
    IDashboardUserRepository userRepository)
    : IDashboardAsyncAuthorizationFilter
{
    public async Task<bool> AuthorizeAsync(DashboardContext context)
    {
        logger.LogInformation("Authorizing request to Hangfire dashboard");
        
        var httpContext = context.GetHttpContext();

        var header = httpContext.Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(header) || !header.StartsWith("Basic "))
        {
            httpContext.Response.Headers.WWWAuthenticate = "Basic";
            return false;
        }
        
        logger.LogInformation("Basic auth header found");

        try
        {
            var authValues = System.Text.Encoding.UTF8.GetString(
                Convert.FromBase64String(header[6..])).Split(':');

            var providedUsername = authValues[0];
            var providedPassword = authValues[1];

            logger.LogInformation("Authenticating user {Username}", providedUsername);
            
            var user = await userRepository.GetUserByUsername(providedUsername);
            if (user == null)
            {
                logger.LogWarning("User {Username} not found", providedUsername);
                return false;
            }

            var isValid = Verify(providedPassword, user.PasswordHash);
            
            logger.LogInformation("User {Username} authentication {Status}", providedUsername, isValid ? "succeeded" : "failed");
            
            if (isValid)
                await userRepository.UpdateLastLoginTime(user.Username, DateTime.UtcNow);

            return isValid;
        }
        catch(Exception ex)
        {
            logger.LogWarning(ex, "Error during authentication");
            return false;
        }
    }
}

public static class AuthorizationFilterExtensions
{
    public static IServiceCollection AddAuthorizationFilter(this IServiceCollection services, string connectionString)
        => services
            .AddSingleton<IDashboardUserRepository>(new DashboardUserRepository(connectionString))
            .AddScoped<IDashboardAsyncAuthorizationFilter, AuthorizationFilter>();
}