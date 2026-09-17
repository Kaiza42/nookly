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
        bool isLiked,
        MediaType? mediaType)
    {
        ExternalSource = externalSource;
        ExternalId = externalId;
        IsLiked = isLiked;
        MediaType = mediaType;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public int Id { get; private set; }
    public string ExternalSource { get; private set; } = string.Empty;
    public string ExternalId { get; private set; } = string.Empty;
    public bool IsLiked { get; private set; }
    public MediaType? MediaType { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static DiscoveryPreference Create(
        string source,
        string externalId,
        bool isLiked,
        MediaType? mediaType) =>
        new(source.Trim().ToLowerInvariant(), externalId.Trim(), isLiked, mediaType);

    public void Update(bool isLiked, MediaType? mediaType)
    {
        IsLiked = isLiked;
        MediaType = mediaType ?? MediaType;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
