using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Nookly.Desktop.Services;
using Nookly.Desktop.ViewModels;

namespace Nookly.Desktop;

public partial class App : System.Windows.Application
{
    private ServiceProvider? serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        var apiBaseAddress = Environment.GetEnvironmentVariable("NOOKLY_API_URL")
                             ?? "http://localhost:5186/";

        services.AddSingleton<SessionStore>();
        services.AddSingleton<ThemeService>();
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
        services.AddHttpClient<IBankApiClient, BankApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseAddress);
            client.Timeout = TimeSpan.FromSeconds(10);
        }).AddHttpMessageHandler<AuthenticatedHttpHandler>();
        services.AddHttpClient<AdminApiClient>(client => { client.BaseAddress = new Uri(apiBaseAddress); client.Timeout = TimeSpan.FromSeconds(10); })
            .AddHttpMessageHandler<AuthenticatedHttpHandler>();
        services.AddHttpClient<MemberApiClient>(client => { client.BaseAddress = new Uri(apiBaseAddress); client.Timeout = TimeSpan.FromSeconds(10); })
            .AddHttpMessageHandler<AuthenticatedHttpHandler>();
        services.AddSingleton<IUserDialogService, UserDialogService>();
        services.AddTransient<LibraryViewModel>();
        services.AddTransient<AdminViewModel>();
        services.AddTransient<MainWindow>();
        services.AddTransient<LoginWindow>();
        services.AddTransient<WelcomeWindow>();
        services.AddTransient<PasswordResetWindow>();

        serviceProvider = services.BuildServiceProvider();
        var session = serviceProvider.GetRequiredService<SessionStore>();
        session.Load();
        if (session.AccessToken is null) serviceProvider.GetRequiredService<LoginWindow>().Show();
        else ShowMainWindow();
    }

    public void ShowMainWindow()
    {
        var provider = serviceProvider ?? throw new InvalidOperationException("Application services are unavailable.");
        var session = provider.GetRequiredService<SessionStore>();
        if (session.Member is not null) provider.GetRequiredService<ThemeService>().Load(session.Member.Id);
        Window window = provider.GetRequiredService<MainWindow>();
        MainWindow = window;
        window.Show();
    }

    public void ShowLoginWindow() => serviceProvider!.GetRequiredService<LoginWindow>().Show();
    public void ShowPasswordResetWindow(Window owner)
    { var window = serviceProvider!.GetRequiredService<PasswordResetWindow>(); window.Owner = owner; window.ShowDialog(); }

    public async Task ShowWelcomeThenMainAsync(string displayName)
    {
        var provider = serviceProvider ?? throw new InvalidOperationException("Application services are unavailable.");
        var session = provider.GetRequiredService<SessionStore>();
        if (session.Member is not null) provider.GetRequiredService<ThemeService>().Load(session.Member.Id);
        var welcome = provider.GetRequiredService<WelcomeWindow>();
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
