namespace Nookly.Contracts.Details;

public sealed record SeasonProgressResponse(
    int SeasonNumber,
    string? PersonalNotes,
    int WatchedEpisodes,
    int TotalEpisodes,
    IReadOnlyList<EpisodeProgressResponse> Episodes);

public sealed record EpisodeProgressResponse(
    int EpisodeNumber,
    bool IsWatched,
    string? PersonalNotes);

public sealed record UpdateSeasonProgressRequest(string? PersonalNotes);
public sealed record UpdateEpisodeProgressRequest(bool IsWatched, string? PersonalNotes);
