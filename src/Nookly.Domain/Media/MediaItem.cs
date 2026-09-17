namespace Nookly.Domain.Media;

public sealed class MediaItem
{
    private MediaItem()
    {
    }

    private MediaItem(Guid id, string title, MediaType type, string? description)
    {
        Id = id;
        Title = title;
        Type = type;
        Description = description;
        Status = MediaStatus.Planned;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid? MemberId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public MediaType Type { get; private set; }
    public MediaStatus Status { get; private set; }
    public string? ExternalSource { get; private set; }
    public string? ExternalId { get; private set; }
    public string? PosterUrl { get; private set; }
    public decimal? CommunityRating { get; private set; }
    public decimal? PersonalRating { get; private set; }
    public string? PersonalNotes { get; private set; }
    public bool IsFavorite { get; private set; }
    public int? CurrentSeason { get; private set; }
    public int? CurrentEpisode { get; private set; }
    public DateOnly? ReleaseDate { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static MediaItem Create(string title, MediaType type, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("A title is required.", nameof(title));
        }

        return new MediaItem(Guid.NewGuid(), title.Trim(), type, description?.Trim());
    }

    public void AssignTo(Guid memberId) => MemberId = memberId;

    public void AttachExternalMetadata(
        string externalSource,
        string externalId,
        string? posterUrl,
        decimal? communityRating,
        DateOnly? releaseDate)
    {
        if (string.IsNullOrWhiteSpace(externalSource) || string.IsNullOrWhiteSpace(externalId))
        {
            throw new ArgumentException("External source and identifier are required together.");
        }

        ExternalSource = externalSource.Trim();
        ExternalId = externalId.Trim();
        PosterUrl = posterUrl;
        CommunityRating = communityRating;
        ReleaseDate = releaseDate;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void UpdatePersonalTracking(
        MediaStatus status,
        decimal? personalRating,
        string? personalNotes,
        bool isFavorite = false,
        int? currentSeason = null,
        int? currentEpisode = null)
    {
        if (personalRating is < 0 or > 10)
        {
            throw new ArgumentOutOfRangeException(
                nameof(personalRating),
                "The personal rating must be between 0 and 10.");
        }

        if (currentSeason is < 0 || currentEpisode is < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(currentSeason),
                "The season and episode must be positive numbers.");
        }

        if (Type is MediaType.Movie or MediaType.Manga)
        {
            currentSeason = null;
            currentEpisode = null;
        }

        Status = status;
        PersonalRating = personalRating;
        PersonalNotes = string.IsNullOrWhiteSpace(personalNotes) ? null : personalNotes.Trim();
        IsFavorite = isFavorite;
        CurrentSeason = currentSeason;
        CurrentEpisode = currentEpisode;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
