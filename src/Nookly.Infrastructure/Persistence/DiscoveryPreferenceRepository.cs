using Microsoft.EntityFrameworkCore;
using Nookly.Application.Abstractions;
using Nookly.Domain.Discovery;
using Nookly.Infrastructure.Data;

namespace Nookly.Infrastructure.Persistence;

internal sealed class DiscoveryPreferenceRepository(NooklyDbContext dbContext)
    : IDiscoveryPreferenceRepository
{
    public Task<DiscoveryPreference?> GetAsync(
        string source,
        string externalId,
        CancellationToken cancellationToken) =>
        dbContext.DiscoveryPreferences.SingleOrDefaultAsync(
            item => item.ExternalSource == source && item.ExternalId == externalId,
            cancellationToken);

    public async Task<IReadOnlySet<string>> GetDislikedIdsAsync(
        string source,
        CancellationToken cancellationToken)
    {
        var ids = await dbContext.DiscoveryPreferences
            .Where(item => item.ExternalSource == source && !item.IsLiked)
            .Select(item => item.ExternalId)
            .ToArrayAsync(cancellationToken);
        return ids.ToHashSet(StringComparer.Ordinal);
    }

    public async Task<IReadOnlyList<DiscoveryPreference>> GetLikedAsync(
        string source,
        CancellationToken cancellationToken) =>
        await dbContext.DiscoveryPreferences
            .AsNoTracking()
            .Where(item => item.ExternalSource == source && item.IsLiked)
            .OrderByDescending(item => item.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<DiscoveryPreference>> ListDislikedAsync(
        CancellationToken cancellationToken) =>
        await dbContext.DiscoveryPreferences
            .Where(item => !item.IsLiked)
            .OrderByDescending(item => item.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task SaveAsync(DiscoveryPreference preference, CancellationToken cancellationToken)
    {
        if (preference.Id == 0) dbContext.DiscoveryPreferences.Add(preference);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(DiscoveryPreference preference, CancellationToken cancellationToken)
    {
        dbContext.DiscoveryPreferences.Remove(preference);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
