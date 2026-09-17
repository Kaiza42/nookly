using Nookly.Application.Abstractions;
using Nookly.Domain.Discovery;

namespace Nookly.Application.Discovery;

public sealed class DiscoveryPreferenceService(IDiscoveryPreferenceRepository repository)
{
    public Task<IReadOnlySet<string>> GetDislikedIdsAsync(
        string source,
        CancellationToken cancellationToken = default) =>
        repository.GetDislikedIdsAsync(source, cancellationToken);

    public async Task SaveAsync(
        string source,
        string externalId,
        bool isLiked,
        CancellationToken cancellationToken = default)
    {
        var preference = await repository.GetAsync(source, externalId, cancellationToken);
        if (preference is null)
        {
            preference = DiscoveryPreference.Create(source, externalId, isLiked);
        }
        else
        {
            preference.Update(isLiked);
        }

        await repository.SaveAsync(preference, cancellationToken);
    }
}
