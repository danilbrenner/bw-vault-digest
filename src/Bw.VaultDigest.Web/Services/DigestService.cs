using Bw.VaultDigest.Infrastructure;

namespace Bw.VaultDigest.Web.Services;

public class DigestService(ILoginProviderAdapter adapter, IEmailTemplateLoader templateLoader, IEmailNotifier notifier)
{
    public async Task CreateDigest()
    {
        var set = await adapter.GetLogins();
            var template = await templateLoader.RenderMessage(set.Logins.Count, set.UserEmail, DateTime.Today);

        await notifier.SendEmail(
            template,
            [
                ("age-diagram", set.Logins.ToAgeSlices().ToDoughnutDiagram()),
                ("complexity-diagram", set.Logins.ToStrengthSlices().ToDoughnutDiagram())
            ]);
    }
}