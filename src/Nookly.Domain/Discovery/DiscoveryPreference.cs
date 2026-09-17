namespace Nookly.Domain.Discovery;

using Nookly.Domain.Media;

public sealed class DiscoveryPreference
{
    private DiscoveryPreference()
    {
    }

    private DiscoveryPreference(
        string externalSource,
        string externalId,
        string title,
        bool isLiked,
        MediaType? mediaType)
    {
        ExternalSource = externalSource;
        ExternalId = externalId;
        Title = title;
        IsLiked = isLiked;
        MediaType = mediaType;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public int Id { get; private set; }
    public Guid? MemberId { get; private set; }
    public string ExternalSource { get; private set; } = string.Empty;
    public string ExternalId { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public bool IsLiked { get; private set; }
    public MediaType? MediaType { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static DiscoveryPreference Create(
        string source,
        string externalId,
        string title,
        bool isLiked,
        MediaType? mediaType) =>
        new(source.Trim().ToLowerInvariant(), externalId.Trim(), title.Trim(), isLiked, mediaType);

    public void AssignTo(Guid memberId) => MemberId = memberId;

    public void Update(string title, bool isLiked, MediaType? mediaType)
    {
        Title = string.IsNullOrWhiteSpace(title) ? Title : title.Trim();
        IsLiked = isLiked;
        MediaType = mediaType ?? MediaType;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
