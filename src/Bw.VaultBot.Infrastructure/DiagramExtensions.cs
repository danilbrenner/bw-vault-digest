using Bw.VaultBot.Model;
using ScottPlot;

namespace Bw.VaultBot.Infrastructure.EmailNotifierClient;

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
                .GroupBy(l => l.Strength)
                .Select(g => new PieSlice { FillColor = g.Key.ToColor(), Value = g.Count() })
                .ToList();
    }

    public static IList<PieSlice> ToAgeSlices(this IReadOnlyList<Login> logins)
    {
        return
            logins
                .GroupBy(l => l.Age)
                .Select(g => new PieSlice { FillColor = g.Key.ToColor(), Value = g.Count() })
                .ToList();
    }

    public static byte[] ToDoughnutDiagram(this IList<PieSlice> slices)
    {
        var plt = new Plot();
        var pie = plt.Add.Pie(slices);
        pie.DonutFraction = .2;
        pie.SliceLabelDistance = 0;
        pie.Padding = -1;
        pie.ExplodeFraction = 0;
        
        plt.Layout.Fixed(new PixelPadding(0));
        
        plt.Grid.IsVisible = false;
        plt.FigureBackground = new BackgroundStyle { Color = Colors.Transparent };
        
        return plt.GetImageBytes(300, 300, ImageFormat.Png);
    }
}