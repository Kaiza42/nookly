using Nookly.Contracts.Media;
using Nookly.Contracts.Search;

namespace Nookly.Desktop.Services;

public interface IMediaApiClient
{
    Task<IReadOnlyList<MediaItemResponse>> GetMediaAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MediaSearchResultResponse>> SearchMediaAsync(
        string query,
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
