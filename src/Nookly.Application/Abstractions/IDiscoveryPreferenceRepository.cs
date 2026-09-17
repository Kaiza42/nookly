using Nookly.Domain.Discovery;

namespace Nookly.Application.Abstractions;

public interface IDiscoveryPreferenceRepository
{
    Task<DiscoveryPreference?> GetAsync(string source, string externalId, CancellationToken cancellationToken);
    Task<IReadOnlySet<string>> GetDislikedIdsAsync(string source, CancellationToken cancellationToken);
    Task<IReadOnlyList<DiscoveryPreference>> GetLikedAsync(string source, CancellationToken cancellationToken);
    Task<IReadOnlyList<DiscoveryPreference>> ListDislikedAsync(CancellationToken cancellationToken);
    Task SaveAsync(DiscoveryPreference preference, CancellationToken cancellationToken);
    Task DeleteAsync(DiscoveryPreference preference, CancellationToken cancellationToken);
}
