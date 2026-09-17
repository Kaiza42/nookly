using Microsoft.EntityFrameworkCore;
using Nookly.Domain.Media;
using Nookly.Domain.Discovery;

namespace Nookly.Infrastructure.Data;

public sealed class NooklyDbContext(DbContextOptions<NooklyDbContext> options)
    : DbContext(options)
{
    public DbSet<MediaItem> MediaItems => Set<MediaItem>();
    public DbSet<DiscoveryPreference> DiscoveryPreferences => Set<DiscoveryPreference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NooklyDbContext).Assembly);
    }
}
