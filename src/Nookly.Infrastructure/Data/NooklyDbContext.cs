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
    public DbSet<SeasonProgress> SeasonProgress => Set<SeasonProgress>();
    public DbSet<EpisodeProgress> EpisodeProgress => Set<EpisodeProgress>();
    public DbSet<DiscoveryPreference> DiscoveryPreferences => Set<DiscoveryPreference>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<BankEntry> BankEntries => Set<BankEntry>();
    public DbSet<AccountToken> AccountTokens => Set<AccountToken>();
    public DbSet<MemberNotification> MemberNotifications => Set<MemberNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NooklyDbContext).Assembly);
    }
}
