using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Nookly.Desktop;

public partial class ColorPickerWindow : Window
{
    private double hue;
    private double saturation;
    private double brightness;
    private bool isSelecting;

    public ColorPickerWindow(string initialColor)
    {
        InitializeComponent();
        var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(initialColor);
        RgbToHsv(color, out hue, out saturation, out brightness);
        Loaded += (_, _) =>
        {
            HueSlider.Value = hue;
            UpdateHueSurface();
            UpdateSelectionMarker();
            UpdateSelectedColor();
        };
    }

    public string SelectedHex { get; private set; } = "#FFFFFF";

    private void ColorArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        isSelecting = true;
        ColorArea.CaptureMouse();
        SelectFromPosition(e.GetPosition(ColorArea));
    }

    private void ColorArea_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (isSelecting) SelectFromPosition(e.GetPosition(ColorArea));
    }

    private void ColorArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!isSelecting) return;
        SelectFromPosition(e.GetPosition(ColorArea));
        isSelecting = false;
        ColorArea.ReleaseMouseCapture();
    }

    private void HueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        hue = e.NewValue;
        if (!IsLoaded) return;
        UpdateHueSurface();
        UpdateSelectedColor();
    }

    private void SelectFromPosition(System.Windows.Point point)
    {
        saturation = Math.Clamp(point.X / Math.Max(1, ColorArea.ActualWidth), 0, 1);
        brightness = 1 - Math.Clamp(point.Y / Math.Max(1, ColorArea.ActualHeight), 0, 1);
        UpdateSelectionMarker();
        UpdateSelectedColor();
    }

    private void UpdateHueSurface() => HueSurface.Background = new SolidColorBrush(HsvToRgb(hue, 1, 1));

    private void UpdateSelectionMarker()
    {
        SelectionMarker.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
        SelectionMarker.VerticalAlignment = System.Windows.VerticalAlignment.Top;
        SelectionMarker.Margin = new Thickness(
            saturation * Math.Max(0, ColorArea.ActualWidth - SelectionMarker.Width),
            (1 - brightness) * Math.Max(0, ColorArea.ActualHeight - SelectionMarker.Height), 0, 0);
    }

    private void UpdateSelectedColor()
    {
        var color = HsvToRgb(hue, saturation, brightness);
        SelectedHex = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        HexText.Text = SelectedHex;
        ColorPreview.Background = new SolidColorBrush(color);
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    private void Confirm_Click(object sender, RoutedEventArgs e) => DialogResult = true;

    private static System.Windows.Media.Color HsvToRgb(double h, double s, double v)
    {
        var chroma = v * s;
        var x = chroma * (1 - Math.Abs(h / 60 % 2 - 1));
        var m = v - chroma;
        var (r, g, b) = h switch
        {
            < 60 => (chroma, x, 0d),
            < 120 => (x, chroma, 0d),
            < 180 => (0d, chroma, x),
            < 240 => (0d, x, chroma),
            < 300 => (x, 0d, chroma),
            _ => (chroma, 0d, x)
        };
        return System.Windows.Media.Color.FromRgb((byte)Math.Round((r + m) * 255),
            (byte)Math.Round((g + m) * 255), (byte)Math.Round((b + m) * 255));
    }

    private static void RgbToHsv(System.Windows.Media.Color color, out double h, out double s, out double v)
    {
        var r = color.R / 255d;
        var g = color.G / 255d;
        var b = color.B / 255d;
        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var delta = max - min;
        h = delta == 0 ? 0 : max == r ? 60 * (((g - b) / delta) % 6)
            : max == g ? 60 * ((b - r) / delta + 2) : 60 * ((r - g) / delta + 4);
        if (h < 0) h += 360;
        s = max == 0 ? 0 : delta / max;
        v = max;
    }
}
