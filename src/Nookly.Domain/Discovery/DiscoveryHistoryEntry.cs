using Nookly.Domain.Media;

namespace Nookly.Domain.Discovery;

public sealed class DiscoveryHistoryEntry
{
    private DiscoveryHistoryEntry() { }

    private DiscoveryHistoryEntry(Guid memberId, string source, string externalId, MediaType mediaType)
    {
        Id = Guid.NewGuid();
        MemberId = memberId;
        ExternalSource = source.Trim().ToLowerInvariant();
        ExternalId = externalId.Trim();
        MediaType = mediaType;
    }

    public Guid Id { get; private set; }
    public Guid MemberId { get; private set; }
    public string ExternalSource { get; private set; } = string.Empty;
    public string ExternalId { get; private set; } = string.Empty;
    public MediaType MediaType { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? PosterUrl { get; private set; }
    public decimal? CommunityRating { get; private set; }
    public DateOnly? ReleaseDate { get; private set; }
    public string? CastLabel { get; private set; }
    public DateTimeOffset ViewedAtUtc { get; private set; }

    public static DiscoveryHistoryEntry Create(
        Guid memberId,
        string source,
        string externalId,
        MediaType mediaType) => new(memberId, source, externalId, mediaType);

    public void RecordView(
        string title,
        string? description,
        string? posterUrl,
        decimal? communityRating,
        DateOnly? releaseDate,
        IReadOnlyList<string> cast,
        DateTimeOffset viewedAtUtc)
    {
        Title = title.Trim();
        Description = description;
        PosterUrl = posterUrl;
        CommunityRating = communityRating;
        ReleaseDate = releaseDate;
        CastLabel = cast.Count > 0 ? string.Join(", ", cast.Take(5)) : null;
        ViewedAtUtc = viewedAtUtc;
    }
}
