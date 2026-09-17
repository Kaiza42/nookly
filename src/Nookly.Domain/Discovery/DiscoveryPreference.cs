namespace Nookly.Domain.Discovery;

public sealed class DiscoveryPreference
{
    private DiscoveryPreference()
    {
    }

    private DiscoveryPreference(string externalSource, string externalId, bool isLiked)
    {
        ExternalSource = externalSource;
        ExternalId = externalId;
        IsLiked = isLiked;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public int Id { get; private set; }
    public string ExternalSource { get; private set; } = string.Empty;
    public string ExternalId { get; private set; } = string.Empty;
    public bool IsLiked { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static DiscoveryPreference Create(string source, string externalId, bool isLiked) =>
        new(source.Trim().ToLowerInvariant(), externalId.Trim(), isLiked);

    public void Update(bool isLiked)
    {
        IsLiked = isLiked;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
