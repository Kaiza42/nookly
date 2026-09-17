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
}
