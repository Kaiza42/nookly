using Nookly.Application.Search;
using Nookly.Application.Details;

namespace Nookly.Application.Abstractions;

public interface IExternalMediaSearch
{
    Task<IReadOnlyList<MediaSearchResult>> SearchAsync(
        string? query,
        IReadOnlyCollection<Nookly.Domain.Media.MediaType>? types = null,
        IReadOnlyCollection<int>? genreIds = null,
        int? year = null,
        string? actor = null,
        string? director = null,
        IReadOnlyCollection<string>? countries = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MediaSearchResult>> RecommendAsync(
        IReadOnlyList<RecommendationSeed> seeds,
        CancellationToken cancellationToken = default);

    Task<string?> GetTitleAsync(
        string externalId,
        Nookly.Domain.Media.MediaType type,
        CancellationToken cancellationToken = default);

    Task<MediaDetails?> GetDetailsAsync(
        string externalId,
        Nookly.Domain.Media.MediaType type,
        CancellationToken cancellationToken = default);

    Task<SeasonDetails?> GetSeasonAsync(
        string externalId,
        int seasonNumber,
        CancellationToken cancellationToken = default);
}
