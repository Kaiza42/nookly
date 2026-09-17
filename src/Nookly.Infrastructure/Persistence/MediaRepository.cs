using Microsoft.EntityFrameworkCore;
using Nookly.Application.Abstractions;
using Nookly.Domain.Media;
using Nookly.Infrastructure.Data;

namespace Nookly.Infrastructure.Persistence;

internal sealed class MediaRepository(NooklyDbContext dbContext) : IMediaRepository
{
    public async Task<IReadOnlyList<MediaItem>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.MediaItems
            .AsNoTracking()
            .OrderByDescending(item => item.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<MediaItem?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return dbContext.MediaItems
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public async Task AddAsync(
        MediaItem mediaItem,
        CancellationToken cancellationToken = default)
    {
        dbContext.MediaItems.Add(mediaItem);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
