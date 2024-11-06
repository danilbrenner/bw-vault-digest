using DotLiquid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bw.VaultDigest.Infrastructure;

public interface IEmailTemplateLoader
{
    Task<string> RenderMessage(int loginsCount, string cratedFor, DateTime createdAt);
}

public class EmailTemplates
{
    public required string Statistics { get; init; }
}

public class EmailTemplateLoader(IOptions<EmailTemplates> options, ILogger<EmailTemplateLoader> logger) :IEmailTemplateLoader
{
    public async Task<string> RenderMessage(int loginsCount, string cratedFor, DateTime createdAt)
    {
        logger.LogTrace("Loading template");
        
        var stringTemplate = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, options.Value.Statistics));
        
        logger.LogTrace("Parsing template");
        
        var template = Template.Parse(stringTemplate);
        
        logger.LogTrace("Rendering template");
        
        return 
            template.Render(
                Hash.FromAnonymousObject(
                    new { loginsCount, cratedFor, createdAt = createdAt.ToString("MM/dd/yyyy") }));

    }
}