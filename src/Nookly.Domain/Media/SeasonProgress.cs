namespace Nookly.Domain.Media;

public sealed class SeasonProgress
{
    private SeasonProgress() { }

    private SeasonProgress(Guid memberId, string externalSource, string externalId, int seasonNumber)
    {
        Id = Guid.NewGuid();
        MemberId = memberId;
        ExternalSource = externalSource.Trim().ToLowerInvariant();
        ExternalId = externalId.Trim();
        SeasonNumber = seasonNumber;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid MemberId { get; private set; }
    public string ExternalSource { get; private set; } = string.Empty;
    public string ExternalId { get; private set; } = string.Empty;
    public int SeasonNumber { get; private set; }
    public string? PersonalNotes { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static SeasonProgress Create(Guid memberId, string source, string externalId, int seasonNumber)
    {
        if (seasonNumber < 1) throw new ArgumentOutOfRangeException(nameof(seasonNumber));
        return new SeasonProgress(memberId, source, externalId, seasonNumber);
    }

    public void SetNotes(string? notes)
    {
        PersonalNotes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
