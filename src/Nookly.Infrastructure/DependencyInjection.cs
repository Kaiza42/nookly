using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nookly.Application.Abstractions;
using Nookly.Application.Media;
using Nookly.Application.Discovery;
using Nookly.Application.Banking;
using Nookly.Infrastructure.Data;
using Nookly.Infrastructure.External.Tmdb;
using Nookly.Infrastructure.Persistence;

namespace Nookly.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Connection string 'Database' is missing.");

        services.AddDbContext<NooklyDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IMediaRepository, MediaRepository>();
        services.AddScoped<IDiscoveryPreferenceRepository, DiscoveryPreferenceRepository>();
        services.AddScoped<DiscoveryPreferenceService>();
        services.AddScoped<IDiscoveryHistoryRepository, DiscoveryHistoryRepository>();
        services.AddScoped<DiscoveryHistoryService>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddScoped<IBankRepository, BankRepository>();
        services.AddScoped<BankService>();
        services.Configure<TmdbOptions>(configuration.GetSection(TmdbOptions.SectionName));
        services.AddHttpClient<IExternalMediaSearch, TmdbClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        return services;
    }
}
