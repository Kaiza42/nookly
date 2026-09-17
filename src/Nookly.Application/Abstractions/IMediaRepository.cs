using Nookly.Domain.Media;

namespace Nookly.Application.Abstractions;

public interface IMediaRepository
{
    Task<IReadOnlyList<MediaItem>> ListAsync(CancellationToken cancellationToken = default);
    Task<MediaItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByExternalIdAsync(
        string externalSource,
        string externalId,
        CancellationToken cancellationToken = default);
    Task AddAsync(MediaItem mediaItem, CancellationToken cancellationToken = default);
    Task UpdateAsync(MediaItem mediaItem, CancellationToken cancellationToken = default);
    Task DeleteAsync(MediaItem mediaItem, CancellationToken cancellationToken = default);
}
