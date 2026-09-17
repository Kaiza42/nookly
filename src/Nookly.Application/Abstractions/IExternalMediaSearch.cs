using Nookly.Application.Search;

namespace Nookly.Application.Abstractions;

public interface IExternalMediaSearch
{
    Task<IReadOnlyList<MediaSearchResult>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default);
}
