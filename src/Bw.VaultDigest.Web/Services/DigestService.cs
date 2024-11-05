using Bw.VaultDigest.Infrastructure;

namespace Bw.VaultDigest.Web.Services;

public class DigestService(
    ILoginProviderAdapter adapter,
    IEmailTemplateLoader templateLoader,
    IEmailNotifier notifier,
    ILogger<DigestService> logger)
{
    public async Task CreateDigest()
    {
        logger.LogTrace("Getting logins");
        
        var set = await adapter.GetLogins();

        logger.LogTrace("Getting email template");
        
        var template = await templateLoader.RenderMessage(set.Logins.Count, set.UserEmail, DateTime.Today);

        logger.LogTrace("Sending email");
        
        await notifier.SendEmail(
            template,
            [
                ("age-diagram", set.Logins.ToAgeSlices().ToDoughnutDiagram()),
                ("complexity-diagram", set.Logins.ToStrengthSlices().ToDoughnutDiagram())
            ]);
        
        logger.LogTrace("Digest sent");
    }
}