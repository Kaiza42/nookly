using Nookly.Domain.Discovery;
using Nookly.Domain.Media;

namespace Nookly.Application.Abstractions;

public interface IDiscoveryHistoryRepository
{
    Task<DiscoveryHistoryEntry?> GetAsync(
        string source,
        string externalId,
        MediaType mediaType,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<DiscoveryHistoryEntry>> ListAsync(CancellationToken cancellationToken);
    Task SaveAsync(DiscoveryHistoryEntry entry, CancellationToken cancellationToken);
}
