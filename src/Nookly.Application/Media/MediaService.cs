using Nookly.Application.Abstractions;
using Nookly.Domain.Media;

namespace Nookly.Application.Media;

public sealed class MediaService(IMediaRepository repository) : IMediaService
{
    public async Task<IReadOnlyList<MediaItemDto>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var items = await repository.ListAsync(cancellationToken);
        return items.Select(ToDto).ToArray();
    }

    public async Task<MediaItemDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var item = await repository.GetByIdAsync(id, cancellationToken);
        return item is null ? null : ToDto(item);
    }

    public async Task<MediaItemDto> CreateAsync(
        CreateMediaRequest request,
        CancellationToken cancellationToken = default)
    {
        var item = MediaItem.Create(request.Title, request.Type, request.Description);
        await repository.AddAsync(item, cancellationToken);
        return ToDto(item);
    }

    private static MediaItemDto ToDto(MediaItem item) => new(
        item.Id,
        item.Title,
        item.Description,
        item.Type,
        item.Status,
        item.CreatedAtUtc);
}
