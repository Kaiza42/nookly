using Microsoft.EntityFrameworkCore;
using Nookly.Domain.Media;
using Nookly.Domain.Discovery;
using Nookly.Domain.Members;

namespace Nookly.Infrastructure.Data;

public sealed class NooklyDbContext(DbContextOptions<NooklyDbContext> options)
    : DbContext(options)
{
    public DbSet<MediaItem> MediaItems => Set<MediaItem>();
    public DbSet<DiscoveryPreference> DiscoveryPreferences => Set<DiscoveryPreference>();
    public DbSet<Member> Members => Set<Member>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NooklyDbContext).Assembly);
    }
}
