using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using Bw.VaultDigest.Infrastructure.Abstractions;
using Bw.VaultDigest.Infrastructure.Options;
using Bw.VaultDigest.Model;
using DotLiquid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bw.VaultDigest.Infrastructure.EmailNotifierClient;

public class EmailNotifier(IOptions<EmailNotifierOptions> emailOptions, ILogger<EmailNotifier> logger) : IEmailNotifier
{
    private static LinkedResource ToPngAttachment((string, byte[]) attachment)
    {
        var (contentId, content) = attachment;

        var image = new LinkedResource(new MemoryStream(content), MediaTypeNames.Image.Png);
        image.ContentId = contentId;
        image.ContentType = new ContentType("image/png");

        return image;
    }
    
    public async Task SendDigest(LoginsSet set, SendDigestEmailSettings settings)
    {
        var template = await RenderMessage(
            settings.Template,
            set.Logins.Count,
            set.UserEmail,
            DateTime.Today);

        await SendEmail(
            settings,
            template,
            [
                ("age-diagram", set.Logins.ToAgeSlices().ToDoughnutDiagram()),
                ("complexity-diagram", set.Logins.ToStrengthSlices().ToDoughnutDiagram())
            ]);
    }

    private async Task<string> RenderMessage(string path, int loginsCount, string cratedFor, DateTime createdAt)
    {
        logger.LogTrace("Loading template");
        
        var stringTemplate = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, path));
        
        logger.LogTrace("Parsing template");
        
        var template = Template.Parse(stringTemplate);
        
        logger.LogTrace("Rendering template");
        
        return 
            template.Render(
                Hash.FromAnonymousObject(
                    new { loginsCount, cratedFor, createdAt = createdAt.ToString("MM/dd/yyyy") }));

    }
    
    private async Task SendEmail(SendDigestEmailSettings settings, string body, IReadOnlyList<(string, byte[])> images)
    {
        try
        {
            var options = emailOptions.Value;
            logger.LogTrace("Sending email notification to {To}", settings.To);

            var message = new MailMessage(options.From, settings.To, settings.Title, body)
            {
                IsBodyHtml = true
            };

            var alternateView =
                AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);

            images.Select(ToPngAttachment).ToList().ForEach(alternateView.LinkedResources.Add);

            message.AlternateViews.Add(alternateView);

            var client = new SmtpClient(options.SmtpServer, options.SmtpPort)
            {
                Credentials = new NetworkCredential(options.Username, options.Password),
                EnableSsl = true,
                UseDefaultCredentials = false
            };

            await client.SendMailAsync(message);
            logger.LogTrace("Email notification sent");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email notification");
            throw;
        }
    }
}