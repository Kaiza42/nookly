using System.Windows;
using System.Windows.Input;
using Nookly.Contracts.Authentication;
using Nookly.Desktop.Services;

namespace Nookly.Desktop;

public partial class LoginWindow : Window
{
    private readonly AuthenticationApiClient authentication;
    private readonly SessionStore session;
    private bool isRegistering;
    public LoginWindow(AuthenticationApiClient authentication, SessionStore session)
    {
        InitializeComponent(); this.authentication = authentication; this.session = session;
        EmailBox.Text = session.RememberedEmail; StaySignedInBox.IsChecked = session.StaySignedIn;
    }
    private void Switch_Click(object sender, RoutedEventArgs e)
    {
        isRegistering = !isRegistering;
        PseudoBox.Visibility = PseudoLabel.Visibility = isRegistering ? Visibility.Visible : Visibility.Collapsed;
        SubmitButton.Content = isRegistering ? "S'inscrire" : "Se connecter";
        SwitchButton.Content = isRegistering ? "J'ai deja un compte" : "Creer un compte";
        SwitchPrompt.Text = isRegistering ? "Deja inscrit ?" : "Pas encore de compte ?";
        ForgotPasswordButton.Visibility = isRegistering ? Visibility.Collapsed : Visibility.Visible;
        Subtitle.Text = isRegistering ? "Cree ton espace personnel" : "Connecte-toi a ton espace personnel";
        ErrorText.Text = string.Empty;
        ErrorSurface.Visibility = Visibility.Collapsed;
    }
    private async void Submit_Click(object sender, RoutedEventArgs e)
    {
        SubmitButton.IsEnabled = false; ErrorText.Text = string.Empty; ErrorSurface.Visibility = Visibility.Collapsed;
        try
        {
            var stay = StaySignedInBox.IsChecked == true;
            if (isRegistering)
            {
                var result = await authentication.RegisterAsync(new RegisterRequest(EmailBox.Text, PasswordBox.Password, PseudoBox.Text, stay));
                System.Windows.MessageBox.Show(result.Message, "Inscription"); Switch_Click(sender, e); return;
            }
            var response = await authentication.LoginAsync(new LoginRequest(EmailBox.Text, PasswordBox.Password, stay));
            session.Set(response, stay);
            var app = (App)System.Windows.Application.Current;
            await app.ShowWelcomeThenMainAsync(response.Member.DisplayName);
            Close();
        }
        catch (Exception exception) { ErrorText.Text = exception.Message; ErrorSurface.Visibility = Visibility.Visible; }
        finally { SubmitButton.IsEnabled = true; }
    }
    private void ForgotPassword_Click(object sender, RoutedEventArgs e) => ((App)System.Windows.Application.Current).ShowPasswordResetWindow(this);

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2) { ToggleWindowState(); return; }
        if (e.LeftButton == MouseButtonState.Pressed) DragMove();
    }

    private void MinimizeWindow_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void MaxRestoreWindow_Click(object sender, RoutedEventArgs e) => ToggleWindowState();
    private void CloseWindow_Click(object sender, RoutedEventArgs e) => Close();
    private void Window_StateChanged(object? sender, EventArgs e)
    {
        if (MaxRestoreButton is null) return;
        MaxRestoreButton.Content = WindowState == WindowState.Maximized ? "\uE923" : "\uE922";
        MaxRestoreButton.ToolTip = WindowState == WindowState.Maximized ? "Restaurer" : "Agrandir";
    }
    private void ToggleWindowState() =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
}
