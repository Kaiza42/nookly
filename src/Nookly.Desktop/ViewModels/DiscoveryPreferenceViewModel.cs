using Nookly.Contracts.Discovery;
using Nookly.Contracts.Media;

namespace Nookly.Desktop.ViewModels;

public sealed class DiscoveryPreferenceViewModel(DiscoveryPreferenceResponse preference)
{
    public string ExternalSource => preference.ExternalSource;
    public string ExternalId => preference.ExternalId;
    public string Title => preference.Title;
    public string TypeLabel => preference.Type switch
    {
        MediaType.Movie => "Film",
        MediaType.TvSeries => "Serie",
        MediaType.Anime => "Anime",
        MediaType.Manga => "Manga",
        _ => "Media"
    };
}
