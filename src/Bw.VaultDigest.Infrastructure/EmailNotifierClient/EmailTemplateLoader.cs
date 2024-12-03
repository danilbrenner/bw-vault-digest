using Bw.VaultDigest.Infrastructure.Abstractions;
using Bw.VaultDigest.Infrastructure.Options;
using DotLiquid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bw.VaultDigest.Infrastructure.EmailNotifierClient;

public class EmailTemplateLoader(IOptions<EmailTemplatesOptions> options, ILogger<EmailTemplateLoader> logger) :IEmailTemplateLoader
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