using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using Bw.VaultDigest.Infrastructure.Abstractions;
using Bw.VaultDigest.Infrastructure.Options;
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

    public Task SendDigestEmail()
    {
        throw new NotImplementedException();
    }

    public async Task SendEmail(string body, IReadOnlyList<(string, byte[])> images)
    {
        try
        {
            var options = emailOptions.Value;
            logger.LogTrace("Sending email notification to {To}", options.To);

            var message = new MailMessage(options.From, options.To, options.DigestTitle, body)
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