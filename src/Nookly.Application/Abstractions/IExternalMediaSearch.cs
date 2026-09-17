using Nookly.Application.Search;

namespace Nookly.Application.Abstractions;

public interface IExternalMediaSearch
{
    Task<IReadOnlyList<MediaSearchResult>> SearchAsync(
        string? query,
        Nookly.Domain.Media.MediaType? type = null,
        int? genreId = null,
        int? year = null,
        string? actor = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MediaSearchResult>> RecommendAsync(
        IReadOnlyList<RecommendationSeed> seeds,
        CancellationToken cancellationToken = default);

    Task<string?> GetTitleAsync(
        string externalId,
        Nookly.Domain.Media.MediaType type,
        CancellationToken cancellationToken = default);
}
