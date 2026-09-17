using Nookly.Application.Abstractions;
using Nookly.Domain.Discovery;
using Nookly.Domain.Media;
using Nookly.Application.Search;

namespace Nookly.Application.Discovery;

public sealed class DiscoveryPreferenceService(IDiscoveryPreferenceRepository repository)
{
    public Task<IReadOnlySet<string>> GetDislikedIdsAsync(
        string source,
        CancellationToken cancellationToken = default) =>
        repository.GetDislikedIdsAsync(source, cancellationToken);

    public async Task<IReadOnlyList<RecommendationSeed>> GetRecommendationSeedsAsync(
        string source,
        CancellationToken cancellationToken = default)
    {
        var preferences = await repository.GetLikedAsync(source, cancellationToken);
        return preferences
            .Where(item => item.MediaType is not null)
            .Select(item => new RecommendationSeed(item.ExternalId, item.MediaType!.Value))
            .Take(3)
            .ToArray();
    }

    public async Task SaveAsync(
        string source,
        string externalId,
        string title,
        bool isLiked,
        MediaType mediaType,
        CancellationToken cancellationToken = default)
    {
        var preference = await repository.GetAsync(source, externalId, cancellationToken);
        if (preference is null)
        {
            preference = DiscoveryPreference.Create(source, externalId, title, isLiked, mediaType);
        }
        else
        {
            preference.Update(title, isLiked, mediaType);
        }

        await repository.SaveAsync(preference, cancellationToken);
    }

    public async Task<IReadOnlyList<DiscoveryPreferenceDto>> ListDislikedAsync(
        CancellationToken cancellationToken = default)
    {
        var preferences = await repository.ListDislikedAsync(cancellationToken);
        return preferences.Select(item => new DiscoveryPreferenceDto(
            item.ExternalSource,
            item.ExternalId,
            string.IsNullOrWhiteSpace(item.Title) ? item.ExternalId : item.Title,
            item.MediaType,
            item.IsLiked)).ToArray();
    }

    public async Task<bool> RestoreAsync(
        string source,
        string externalId,
        CancellationToken cancellationToken = default)
    {
        var preference = await repository.GetAsync(source, externalId, cancellationToken);
        if (preference is null || preference.IsLiked) return false;
        await repository.DeleteAsync(preference, cancellationToken);
        return true;
    }
}
