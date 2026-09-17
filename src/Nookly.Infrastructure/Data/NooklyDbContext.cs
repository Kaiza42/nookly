using Microsoft.EntityFrameworkCore;
using Nookly.Domain.Media;

namespace Nookly.Infrastructure.Data;

public sealed class NooklyDbContext(DbContextOptions<NooklyDbContext> options)
    : DbContext(options)
{
    public DbSet<MediaItem> MediaItems => Set<MediaItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NooklyDbContext).Assembly);
    }
}
