using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bw.VaultDigest.Infrastructure;

public interface IEmailNotifier
{
    Task SendEmail(string to, string subject, string body, IReadOnlyList<(string, byte[])> images);
}

public class EmailNotifierOptions
{
    public required string From { get; init; }
    public required string SmtpServer { get; init; }
    public required int SmtpPort { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
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

    public async Task SendEmail(string to, string subject, string body, IReadOnlyList<(string, byte[])> images)
    {
        logger.LogInformation("Sending email notification to {To}", to);
        var options = emailOptions.Value;

        var message = new MailMessage(options.From, to, subject, body)
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

        try
        {
            await client.SendMailAsync(message);
            logger.LogInformation("Email notification sent");
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Failed to send email notification");
            throw;
        }
    }
}