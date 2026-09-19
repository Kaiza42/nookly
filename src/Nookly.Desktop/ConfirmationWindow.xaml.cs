using System.Windows;

namespace Nookly.Desktop;

public partial class ConfirmationWindow : Window
{
    public ConfirmationWindow(string mediaTitle)
    {
        InitializeComponent();
        ConfirmationText.Text = $"Retirer \"{mediaTitle}\" de ta bibliotheque ?";
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void Confirm_Click(object sender, RoutedEventArgs e) => DialogResult = true;
}
