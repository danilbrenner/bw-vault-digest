using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Bw.VaultBot.Api
{
    public class BasicAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _username;
        private readonly string _password;

        public BasicAuthMiddleware(RequestDelegate next)
        {
            _next = next;
            _username = "admin";
            _password = "123";
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // if (context.Request.Path.StartsWithSegments("/api"))
            // {
            if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"VaultBot API\"");
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            var header = authHeader.ToString();
            if (!header.StartsWith("Basic "))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"VaultBot API\"");
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            var encoded = header.Substring("Basic ".Length).Trim();
            var credentialBytes = Convert.FromBase64String(encoded);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
            if (credentials.Length != 2 || credentials[0] != _username || credentials[1] != _password)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"VaultBot API\"");
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            // }
            await _next(context);
        }
    }
}