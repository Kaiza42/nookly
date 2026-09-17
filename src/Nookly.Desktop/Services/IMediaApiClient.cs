using Nookly.Contracts.Media;
using Nookly.Contracts.Search;
using Nookly.Contracts.Discovery;

namespace Nookly.Desktop.Services;

public interface IMediaApiClient
{
    Task<IReadOnlyList<MediaItemResponse>> GetMediaAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MediaSearchResultResponse>> SearchMediaAsync(
        string? query,
        MediaType? type = null,
        int? genreId = null,
        int? year = null,
        string? actor = null,
        CancellationToken cancellationToken = default);

    Task<MediaItemResponse> CreateMediaAsync(
        CreateMediaRequest request,
        CancellationToken cancellationToken = default);

    Task SetDiscoveryPreferenceAsync(
        SetDiscoveryPreferenceRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DiscoveryPreferenceResponse>> GetDislikedPreferencesAsync(
        CancellationToken cancellationToken = default);

    Task RestoreDiscoveryPreferenceAsync(
        string source,
        string externalId,
        CancellationToken cancellationToken = default);

    Task<MediaItemResponse> UpdateMediaAsync(
        Guid id,
        UpdateMediaRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteMediaAsync(Guid id, CancellationToken cancellationToken = default);
}
