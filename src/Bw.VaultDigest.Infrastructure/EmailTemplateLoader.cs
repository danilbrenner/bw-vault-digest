using DotLiquid;
using Microsoft.Extensions.Options;

namespace Bw.VaultDigest.Infrastructure;

public interface IEmailTemplateLoader
{
    Task<string> GetTemplate(string cratedFor, DateTime createdAt);
}

public class EmailTemplates
{
    public required string Statistics { get; init; }
}

public class EmailTemplateLoader(IOptions<EmailTemplates> options) :IEmailTemplateLoader
{
    public async Task<string> GetTemplate(string cratedFor, DateTime createdAt)
    {
        var stringTemplate = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, options.Value.Statistics));
        var template = Template.Parse(stringTemplate);
        return 
            template.Render(
                Hash.FromAnonymousObject(
                    new { cratedFor, createdAt = createdAt.ToString("MM/dd/yyyy") }));

    }
}