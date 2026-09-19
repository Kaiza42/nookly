using Microsoft.EntityFrameworkCore;
using Nookly.Application.Abstractions;
using Nookly.Domain.Discovery;
using Nookly.Domain.Media;
using Nookly.Infrastructure.Data;

namespace Nookly.Infrastructure.Persistence;

internal sealed class DiscoveryHistoryRepository(
    NooklyDbContext dbContext,
    ICurrentMember currentMember) : IDiscoveryHistoryRepository
{
    public Task<DiscoveryHistoryEntry?> GetAsync(
        string source,
        string externalId,
        MediaType mediaType,
        CancellationToken cancellationToken) =>
        dbContext.DiscoveryHistory.SingleOrDefaultAsync(item =>
            item.MemberId == currentMember.Id &&
            item.ExternalSource == source &&
            item.ExternalId == externalId &&
            item.MediaType == mediaType,
            cancellationToken);

    public async Task<IReadOnlyList<DiscoveryHistoryEntry>> ListAsync(CancellationToken cancellationToken) =>
        await dbContext.DiscoveryHistory
            .AsNoTracking()
            .Where(item => item.MemberId == currentMember.Id)
            .OrderByDescending(item => item.ViewedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task SaveAsync(DiscoveryHistoryEntry entry, CancellationToken cancellationToken)
    {
        if (dbContext.Entry(entry).State == EntityState.Detached) dbContext.DiscoveryHistory.Add(entry);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
