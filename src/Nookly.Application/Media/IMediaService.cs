namespace Nookly.Application.Media;

public interface IMediaService
{
    Task<IReadOnlyList<MediaItemDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<MediaItemDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MediaItemDto> CreateAsync(CreateMediaRequest request, CancellationToken cancellationToken = default);
    Task<MediaItemDto?> UpdateAsync(
        Guid id,
        UpdateMediaRequest request,
        CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
