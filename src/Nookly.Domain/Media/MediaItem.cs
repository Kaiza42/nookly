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
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public MediaType Type { get; private set; }
    public MediaStatus Status { get; private set; }
    public string? ExternalSource { get; private set; }
    public string? ExternalId { get; private set; }
    public string? PosterUrl { get; private set; }
    public decimal? CommunityRating { get; private set; }
    public decimal? PersonalRating { get; private set; }
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

    public void Update(
        string title,
        MediaType type,
        string? description,
        MediaStatus status,
        decimal? personalRating)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("A title is required.", nameof(title));
        }

        if (personalRating is < 0 or > 10)
        {
            throw new ArgumentOutOfRangeException(
                nameof(personalRating),
                "The personal rating must be between 0 and 10.");
        }

        Title = title.Trim();
        Type = type;
        Description = description?.Trim();
        Status = status;
        PersonalRating = personalRating;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
