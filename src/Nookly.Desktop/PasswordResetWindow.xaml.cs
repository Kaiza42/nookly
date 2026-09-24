using System.Windows;
using System.Windows.Input;
using Nookly.Desktop.Services;

namespace Nookly.Desktop;

public partial class PasswordResetWindow : Window
{
    private readonly AuthenticationApiClient authentication;
    public PasswordResetWindow(AuthenticationApiClient authentication) { InitializeComponent(); this.authentication = authentication; }
    private async void SendCode_Click(object sender, RoutedEventArgs e)
    { try { ShowStatus((await authentication.ForgotPasswordAsync(EmailBox.Text)).Message); } catch (Exception ex) { ShowStatus(ex.Message); } }
    private async void Reset_Click(object sender, RoutedEventArgs e)
    { try { ShowStatus((await authentication.ResetPasswordAsync(TokenBox.Text, NewPasswordBox.Password)).Message); } catch (Exception ex) { ShowStatus(ex.Message); } }

    private void ShowStatus(string message)
    {
        StatusText.Text = message;
        StatusSurface.Visibility = Visibility.Visible;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed) DragMove();
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
