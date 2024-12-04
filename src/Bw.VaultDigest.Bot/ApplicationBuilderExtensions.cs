namespace Bw.VaultDigest.Bot;

public static class ApplicationBuilderExtensions
{
    public static HostApplicationBuilder ConfigureServices(this HostApplicationBuilder builder,
        Action<IConfiguration, IServiceCollection> configure)
    {
        configure(builder.Configuration, builder.Services);
        return builder;
    }
}