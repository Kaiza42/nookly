using Microsoft.EntityFrameworkCore;
using Nookly.Domain.Media;
using Nookly.Domain.Discovery;
using Nookly.Domain.Members;
using Nookly.Domain.Banking;

namespace Nookly.Infrastructure.Data;

public sealed class NooklyDbContext(DbContextOptions<NooklyDbContext> options)
    : DbContext(options)
{
    public DbSet<MediaItem> MediaItems => Set<MediaItem>();
    public DbSet<DiscoveryPreference> DiscoveryPreferences => Set<DiscoveryPreference>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<BankEntry> BankEntries => Set<BankEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NooklyDbContext).Assembly);
    }
}
