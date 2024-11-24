namespace Bw.VaultDigest.Web;

public static class ApplicationBuilderExtensions
{
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder,
        Action<IConfiguration, IServiceCollection> configure)
    {
        configure(builder.Configuration, builder.Services);
        return builder;
    }
}