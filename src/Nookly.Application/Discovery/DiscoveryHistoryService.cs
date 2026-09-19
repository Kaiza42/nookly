using Nookly.Application.Abstractions;
using Nookly.Application.Details;
using Nookly.Domain.Discovery;

namespace Nookly.Application.Discovery;

public sealed class DiscoveryHistoryService(
    IDiscoveryHistoryRepository repository,
    ICurrentMember currentMember)
{
    public async Task RecordAsync(
        string source,
        MediaDetails details,
        CancellationToken cancellationToken = default)
    {
        var entry = await repository.GetAsync(source, details.ExternalId, details.Type, cancellationToken)
                    ?? DiscoveryHistoryEntry.Create(currentMember.Id, source, details.ExternalId, details.Type);
        entry.RecordView(
            details.Title,
            details.Description,
            details.PosterUrl,
            details.CommunityRating,
            details.ReleaseDate,
            details.Cast,
            DateTimeOffset.UtcNow);
        await repository.SaveAsync(entry, cancellationToken);
    }

    public async Task<IReadOnlyList<DiscoveryHistoryDto>> ListAsync(
        CancellationToken cancellationToken = default) =>
        (await repository.ListAsync(cancellationToken))
        .Select(item => new DiscoveryHistoryDto(
            item.ExternalSource,
            item.ExternalId,
            item.MediaType,
            item.Title,
            item.Description,
            item.PosterUrl,
            item.CommunityRating,
            item.ReleaseDate,
            item.CastLabel,
            item.ViewedAtUtc))
        .ToArray();
}
