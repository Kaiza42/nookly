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
        if (string.IsNullOrWhiteSpace(request.ExternalSource) ||
            string.IsNullOrWhiteSpace(request.ExternalId))
        {
            throw new ArgumentException(
                "A media must come from an external catalog search.");
        }

        if (!string.Equals(request.ExternalSource, "tmdb", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("This external catalog is not supported.");
        }

        var exists = await repository.ExistsByExternalIdAsync(
            request.ExternalSource,
            request.ExternalId,
            cancellationToken);
        if (exists)
        {
            throw new DuplicateMediaException("This media is already in the library.");
        }

        var item = MediaItem.Create(request.Title, request.Type, request.Description);
        item.AttachExternalMetadata(
            request.ExternalSource,
            request.ExternalId,
            request.PosterUrl,
            request.CommunityRating,
            request.ReleaseDate);

        await repository.AddAsync(item, cancellationToken);
        return ToDto(item);
    }

    public async Task<MediaItemDto?> UpdateAsync(
        Guid id,
        UpdateMediaRequest request,
        CancellationToken cancellationToken = default)
    {
        var item = await repository.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return null;
        }

        item.Update(
            request.Title,
            request.Type,
            request.Description,
            request.Status,
            request.PersonalRating);
        await repository.UpdateAsync(item, cancellationToken);
        return ToDto(item);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var item = await repository.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return false;
        }

        await repository.DeleteAsync(item, cancellationToken);
        return true;
    }

    private static MediaItemDto ToDto(MediaItem item) => new(
        item.Id,
        item.Title,
        item.Description,
        item.Type,
        item.Status,
        item.PersonalRating,
        item.ExternalSource,
        item.ExternalId,
        item.PosterUrl,
        item.CommunityRating,
        item.ReleaseDate,
        item.CreatedAtUtc,
        item.UpdatedAtUtc);
}
