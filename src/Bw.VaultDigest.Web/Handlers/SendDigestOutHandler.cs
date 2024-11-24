using Bw.VaultDigest.Infrastructure;
using Bw.VaultDigest.Model;
using Bw.VaultDigest.Telemetry;
using Bw.VaultDigest.Web.Requests;
using MediatR;
using ScottPlot;

namespace Bw.VaultDigest.Web.Handlers;

public static class DiagramExtensions
{
    public static Color ToColor(this Strength strength)
    {
        return strength switch
        {
            Strength.VeryWeak => Colors.Red,
            Strength.Weak => Colors.Orange,
            Strength.Moderate => Colors.Yellow,
            Strength.Strong => Colors.LightGreen,
            Strength.VeryStrong => Colors.Green,
            _ => throw new ArgumentOutOfRangeException(nameof(strength), strength, null)
        };
    }

    public static Color ToColor(this Age age)
    {
        return age switch
        {
            Age.Ancient => Colors.Red,
            Age.Old => Colors.Orange,
            Age.Moderate => Colors.Yellow,
            Age.Recent => Colors.LightGreen,
            Age.New => Colors.Green,
            _ => throw new ArgumentOutOfRangeException(nameof(age), age, null)
        };
    }

    public static IList<PieSlice> ToStrengthSlices(this IReadOnlyList<Login> logins)
    {
        return
            logins
                .GroupBy(l => l.Password.Strength)
                .Select(g => new PieSlice { FillColor = g.Key.ToColor(), Value = g.Count() })
                .ToList();
    }

    public static IList<PieSlice> ToAgeSlices(this IReadOnlyList<Login> logins)
    {
        return
            logins
                .GroupBy(l => l.Password.Age)
                .Select(g => new PieSlice { FillColor = g.Key.ToColor(), Value = g.Count() })
                .ToList();
    }

    public static byte[] ToDoughnutDiagram(this IList<PieSlice> slices)
    {
        var plt = new Plot();
        var pie = plt.Add.Pie(slices);
        pie.DonutFraction = .6;

        // Hide axis label and tick
        plt.Axes.Bottom.TickLabelStyle.IsVisible = false;
        plt.Axes.Bottom.MajorTickStyle.Length = 0;
        plt.Axes.Bottom.MinorTickStyle.Length = 0;

        // Hide axis label and tick
        plt.Axes.Left.TickLabelStyle.IsVisible = false;
        plt.Axes.Left.MajorTickStyle.Length = 0;
        plt.Axes.Left.MinorTickStyle.Length = 0;

        // Hide axis edge line
        plt.Axes.Bottom.FrameLineStyle.Width = 0;
        plt.Axes.Right.FrameLineStyle.Width = 0;
        plt.Axes.Top.FrameLineStyle.Width = 0;
        plt.Axes.Left.FrameLineStyle.Width = 0;

        plt.Grid.IsVisible = false;
        plt.FigureBackground = new BackgroundStyle { Color = Colors.Transparent };

        return plt.GetImageBytes(300, 300, ImageFormat.Png);
    }
}

public class SendDigestOutHandler(
    MetricsFactory metricsFactory,
    IEmailTemplateLoader templateLoader,
    IEmailNotifier notifier,
    ILogger<SendDigestOutHandler> logger) : INotificationHandler<LoginsSyncedEvent>
{
    public async Task Handle(LoginsSyncedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Sending digest");
        using var _ = metricsFactory.CreateDurationMetric("send-digest.duration");

        var template = await templateLoader.RenderMessage(
            notification.Set.Logins.Count,
            notification.Set.UserEmail,
            DateTime.Today);

        await notifier.SendEmail(
            template,
            [
                ("age-diagram", notification.Set.Logins.ToAgeSlices().ToDoughnutDiagram()),
                ("complexity-diagram", notification.Set.Logins.ToStrengthSlices().ToDoughnutDiagram())
            ]);

        logger.LogInformation("Digest sent");
    }
}