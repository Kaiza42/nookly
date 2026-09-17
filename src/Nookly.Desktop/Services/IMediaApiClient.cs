using Nookly.Contracts.Media;

namespace Nookly.Desktop.Services;

public interface IMediaApiClient
{
    Task<IReadOnlyList<MediaItemResponse>> GetMediaAsync(
        CancellationToken cancellationToken = default);

    Task<MediaItemResponse> CreateMediaAsync(
        CreateMediaRequest request,
        CancellationToken cancellationToken = default);

    Task<MediaItemResponse> UpdateMediaAsync(
        Guid id,
        UpdateMediaRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteMediaAsync(Guid id, CancellationToken cancellationToken = default);
}
