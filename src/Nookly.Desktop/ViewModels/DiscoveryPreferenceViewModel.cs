using Nookly.Contracts.Discovery;
using Nookly.Contracts.Media;

namespace Nookly.Desktop.ViewModels;

public sealed class DiscoveryPreferenceViewModel(DiscoveryPreferenceResponse preference)
{
    public string ExternalSource => preference.ExternalSource;
    public string ExternalId => preference.ExternalId;
    public string Title => preference.Title;
    public string? Description => preference.Description;
    public string? PosterUrl => preference.PosterUrl;
    public string TypeLabel => preference.Type switch
    {
        MediaType.Movie => "Film",
        MediaType.TvSeries => "Serie",
        MediaType.Anime => "Anime",
        MediaType.Manga => "Manga",
        _ => "Media"
    };
    public string MetaLabel => string.Join(" - ", new[]
    {
        TypeLabel,
        preference.ReleaseDate?.Year.ToString(),
        preference.CommunityRating is null ? null : $"{preference.CommunityRating:0.0}/10"
    }.Where(value => value is not null));
    public string CastLabel => preference.Cast is { Count: > 0 }
        ? string.Join(", ", preference.Cast.Take(3))
        : "Distribution non disponible";
}
