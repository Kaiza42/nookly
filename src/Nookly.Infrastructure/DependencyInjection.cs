using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nookly.Application.Abstractions;
using Nookly.Application.Media;
using Nookly.Infrastructure.Data;
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
        services.AddScoped<IMediaService, MediaService>();

        return services;
    }
}
