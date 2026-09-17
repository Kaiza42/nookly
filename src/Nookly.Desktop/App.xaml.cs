using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Nookly.Desktop.Services;
using Nookly.Desktop.ViewModels;

namespace Nookly.Desktop;

public partial class App : Application
{
    private ServiceProvider? serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        var apiBaseAddress = Environment.GetEnvironmentVariable("NOOKLY_API_URL")
                             ?? "http://localhost:5186/";

        services.AddSingleton<SessionStore>();
        services.AddTransient<AuthenticatedHttpHandler>();
        services.AddHttpClient<AuthenticationApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseAddress);
            client.Timeout = TimeSpan.FromSeconds(10);
        });
        services.AddHttpClient<IMediaApiClient, MediaApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseAddress);
            client.Timeout = TimeSpan.FromSeconds(10);
        }).AddHttpMessageHandler<AuthenticatedHttpHandler>();
        services.AddSingleton<IUserDialogService, UserDialogService>();
        services.AddTransient<LibraryViewModel>();
        services.AddTransient<MainWindow>();
        services.AddTransient<LoginWindow>();
        services.AddTransient<WelcomeWindow>();

        serviceProvider = services.BuildServiceProvider();
        var session = serviceProvider.GetRequiredService<SessionStore>();
        session.Load();
        if (session.AccessToken is null) serviceProvider.GetRequiredService<LoginWindow>().Show();
        else ShowMainWindow();
    }

    public void ShowMainWindow()
    {
        var window = serviceProvider!.GetRequiredService<MainWindow>();
        MainWindow = window;
        window.Show();
    }

    public void ShowLoginWindow() => serviceProvider!.GetRequiredService<LoginWindow>().Show();

    public async Task ShowWelcomeThenMainAsync(string displayName)
    {
        var welcome = serviceProvider!.GetRequiredService<WelcomeWindow>();
        welcome.SetDisplayName(displayName);
        welcome.Show();
        await Task.Delay(TimeSpan.FromSeconds(3));
        ShowMainWindow();
        welcome.Close();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
