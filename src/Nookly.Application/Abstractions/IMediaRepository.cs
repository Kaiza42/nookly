using Nookly.Domain.Media;

namespace Nookly.Application.Abstractions;

public interface IMediaRepository
{
    Task<IReadOnlyList<MediaItem>> ListAsync(CancellationToken cancellationToken = default);
    Task<MediaItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(MediaItem mediaItem, CancellationToken cancellationToken = default);
}
