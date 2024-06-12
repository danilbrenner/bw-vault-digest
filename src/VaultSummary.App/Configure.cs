namespace VaultSummary.App;

public static class Configure
{
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder,
        Action<IConfiguration, IServiceCollection> configure)
    {
        configure(builder.Configuration, builder.Services);
        return builder;
    }
}