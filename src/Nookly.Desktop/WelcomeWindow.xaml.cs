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
}
