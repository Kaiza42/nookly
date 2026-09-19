using System.Windows;

namespace Nookly.Desktop;

public partial class WelcomeWindow : Window
{
    public WelcomeWindow()
    {
        InitializeComponent();
    }

    public void SetDisplayName(string displayName) =>
        WelcomeText.Text = $"Bienvenue, {displayName}";

    public void SetGoodbye(string displayName)
    {
        WelcomeText.Text = $"\u00C0 bient\u00F4t, {displayName}";
        SubtitleText.Text = "Merci d'avoir pass\u00E9 un moment avec Nookly.";
    }
}
