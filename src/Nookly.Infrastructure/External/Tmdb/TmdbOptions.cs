namespace Nookly.Infrastructure.External.Tmdb;

public sealed class TmdbOptions
{
    public const string SectionName = "Tmdb";

    public string? ReadAccessToken { get; init; }
}
