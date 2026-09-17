using System.Windows;
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
        Subtitle.Text = isRegistering ? "Cree ton espace personnel" : "Connecte-toi a ton espace personnel";
        ErrorText.Text = string.Empty;
    }
    private async void Submit_Click(object sender, RoutedEventArgs e)
    {
        SubmitButton.IsEnabled = false; ErrorText.Text = string.Empty;
        try
        {
            var stay = StaySignedInBox.IsChecked == true;
            var response = isRegistering
                ? await authentication.RegisterAsync(new RegisterRequest(EmailBox.Text, PasswordBox.Password, PseudoBox.Text, stay))
                : await authentication.LoginAsync(new LoginRequest(EmailBox.Text, PasswordBox.Password, stay));
            session.Set(response, stay);
            var app = (App)Application.Current;
            await app.ShowWelcomeThenMainAsync(response.Member.DisplayName);
            Close();
        }
        catch (Exception exception) { ErrorText.Text = exception.Message; }
        finally { SubmitButton.IsEnabled = true; }
    }
}
