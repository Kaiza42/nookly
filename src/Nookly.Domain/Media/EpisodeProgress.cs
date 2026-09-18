namespace Nookly.Domain.Media;

public sealed class EpisodeProgress
{
    private EpisodeProgress() { }

    private EpisodeProgress(Guid memberId, string externalSource, string externalId, int seasonNumber, int episodeNumber)
    {
        Id = Guid.NewGuid();
        MemberId = memberId;
        ExternalSource = externalSource.Trim().ToLowerInvariant();
        ExternalId = externalId.Trim();
        SeasonNumber = seasonNumber;
        EpisodeNumber = episodeNumber;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid MemberId { get; private set; }
    public string ExternalSource { get; private set; } = string.Empty;
    public string ExternalId { get; private set; } = string.Empty;
    public int SeasonNumber { get; private set; }
    public int EpisodeNumber { get; private set; }
    public bool IsWatched { get; private set; }
    public string? PersonalNotes { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static EpisodeProgress Create(Guid memberId, string source, string externalId, int seasonNumber, int episodeNumber)
    {
        if (seasonNumber < 1) throw new ArgumentOutOfRangeException(nameof(seasonNumber));
        if (episodeNumber < 1) throw new ArgumentOutOfRangeException(nameof(episodeNumber));
        return new EpisodeProgress(memberId, source, externalId, seasonNumber, episodeNumber);
    }

    public void Update(bool isWatched, string? notes)
    {
        IsWatched = isWatched;
        PersonalNotes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
