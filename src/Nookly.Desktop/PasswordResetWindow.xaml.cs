using System.Windows;
using Nookly.Desktop.Services;

namespace Nookly.Desktop;

public partial class PasswordResetWindow : Window
{
    private readonly AuthenticationApiClient authentication;
    public PasswordResetWindow(AuthenticationApiClient authentication) { InitializeComponent(); this.authentication = authentication; }
    private async void SendCode_Click(object sender, RoutedEventArgs e)
    { try { StatusText.Text = (await authentication.ForgotPasswordAsync(EmailBox.Text)).Message; } catch (Exception ex) { StatusText.Text = ex.Message; } }
    private async void Reset_Click(object sender, RoutedEventArgs e)
    { try { StatusText.Text = (await authentication.ResetPasswordAsync(TokenBox.Text, NewPasswordBox.Password)).Message; } catch (Exception ex) { StatusText.Text = ex.Message; } }
}
