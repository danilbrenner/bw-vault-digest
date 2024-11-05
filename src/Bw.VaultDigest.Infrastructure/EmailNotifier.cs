using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bw.VaultDigest.Infrastructure;

public interface IEmailNotifier
{
    Task SendEmail(string body, IReadOnlyList<(string, byte[])> images);
}

public class EmailNotifierOptions
{
    public required string From { get; init; }
    public required string To { get; init; }
    public required string SmtpServer { get; init; }
    public required int SmtpPort { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string DigestTitle { get; init; }
}

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
                EnableSsl = true
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