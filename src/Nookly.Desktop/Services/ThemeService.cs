using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;

namespace Nookly.Desktop.Services;

public sealed record ThemePalette(
    string Name,
    string Background,
    string Surface,
    string Sidebar,
    string Accent,
    string Text,
    string MutedText,
    string Border,
    string? TitleBar = null);

public sealed class ThemeService
{
    private readonly string directory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Nookly", "themes");

    public static IReadOnlyDictionary<string, ThemePalette> Presets { get; } =
        new Dictionary<string, ThemePalette>(StringComparer.OrdinalIgnoreCase)
        {
            ["nookly"] = new("Nookly", "#F5F6F8", "#FFFFFF", "#20242B", "#286C53", "#11161C", "#69717D", "#D7DBE1", "#DDE1E5"),
            ["night"] = new("Nuit", "#171A1F", "#22262D", "#111318", "#5E9B82", "#F1F4F3", "#AAB3BC", "#39414A", "#22262D"),
            ["forest"] = new("Foret", "#EEF2EF", "#FAFCFA", "#1E2924", "#3F765E", "#17201C", "#65736C", "#CBD6D0", "#E3EAE6"),
            ["burgundy"] = new("Bordeaux", "#F4F1F2", "#FFFFFF", "#292126", "#7A3E50", "#21171B", "#78676D", "#D9CDD1", "#EEE7E9"),
            ["ocean"] = new("Ocean", "#EFF3F5", "#FFFFFF", "#1E272D", "#3E7180", "#152026", "#65747C", "#CBD7DC", "#E5ECEF")
        };

    public ThemePalette Current { get; private set; } = Presets["nookly"];

    public void Load(Guid memberId)
    {
        Directory.CreateDirectory(directory);
        var path = GetPath(memberId);
        if (!File.Exists(path))
        {
            Apply(Presets["nookly"]);
            return;
        }

        try
        {
            var palette = JsonSerializer.Deserialize<ThemePalette>(File.ReadAllText(path)) ?? Presets["nookly"];
            Apply(string.Equals(palette.Name, "Nookly", StringComparison.OrdinalIgnoreCase)
                ? Presets["nookly"]
                : palette);
        }
        catch (JsonException)
        {
            Apply(Presets["nookly"]);
        }
    }

    public void ApplyAndSave(Guid memberId, ThemePalette palette)
    {
        Apply(palette);
        Directory.CreateDirectory(directory);
        File.WriteAllText(GetPath(memberId), JsonSerializer.Serialize(palette));
    }

    public static bool IsValidColor(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        try
        {
            return System.Windows.Media.ColorConverter.ConvertFromString(value) is System.Windows.Media.Color;
        }
        catch (Exception exception) when (exception is FormatException or NotSupportedException)
        {
            return false;
        }
    }

    private void Apply(ThemePalette palette)
    {
        Current = palette;
        SetBrush("ThemeBackgroundBrush", palette.Background);
        SetBrush("ThemeSurfaceBrush", palette.Surface);
        SetBrush("ThemeSidebarBrush", palette.Sidebar);
        SetBrush("ThemeAccentBrush", palette.Accent);
        SetBrush("ThemeAccentHoverBrush", Adjust(palette.Accent, 0.82));
        SetBrush("ThemeSidebarHoverBrush", Adjust(palette.Sidebar, 1.18));
        SetBrush("ThemeTextBrush", palette.Text);
        SetBrush("ThemeMutedTextBrush", palette.MutedText);
        SetBrush("ThemeBorderBrush", palette.Border);
        SetBrush("ThemeTitleBarBrush", palette.TitleBar ?? palette.Surface);
    }

    private static void SetBrush(string key, string value) =>
        System.Windows.Application.Current.Resources[key] = new SolidColorBrush(
            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(value));

    private static string Adjust(string value, double factor)
    {
        var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(value);
        static byte Scale(byte channel, double amount) => (byte)Math.Clamp(channel * amount, 0, 255);
        return $"#{Scale(color.R, factor):X2}{Scale(color.G, factor):X2}{Scale(color.B, factor):X2}";
    }

    private string GetPath(Guid memberId) => Path.Combine(directory, $"{memberId:N}.json");
}
