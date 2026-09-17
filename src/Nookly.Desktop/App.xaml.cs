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

        services.AddHttpClient<IMediaApiClient, MediaApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseAddress);
            client.Timeout = TimeSpan.FromSeconds(10);
        });
        services.AddTransient<LibraryViewModel>();
        services.AddTransient<MainWindow>();

        serviceProvider = services.BuildServiceProvider();
        serviceProvider.GetRequiredService<MainWindow>().Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
