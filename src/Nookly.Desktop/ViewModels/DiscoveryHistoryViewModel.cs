using Nookly.Contracts.Discovery;
using Nookly.Contracts.Media;
using Nookly.Contracts.Search;

namespace Nookly.Desktop.ViewModels;

public sealed class DiscoveryHistoryViewModel(DiscoveryHistoryResponse item)
{
    public string Title => item.Title;
    public string? Description => item.Description;
    public string? PosterUrl => item.PosterUrl;
    public string CastLabel => item.CastLabel ?? "Distribution non disponible";
    public string TypeLabel => item.MediaType switch
    {
        MediaType.Movie => "Film",
        MediaType.TvSeries => "Serie",
        MediaType.Anime => "Anime",
        _ => item.MediaType.ToString()
    };
    public string MetaLabel => $"{TypeLabel} - {item.ReleaseDate?.Year.ToString() ?? "Date inconnue"} - " +
                               (item.CommunityRating is null ? "Non note" : $"{item.CommunityRating:0.0}/10");
    public string ViewedAtLabel => $"Consulte le {item.ViewedAtUtc.ToLocalTime():dd/MM/yyyy a HH:mm}";

    public MediaSearchResultResponse ToSearchResult() => new(
        item.ExternalSource,
        item.ExternalId,
        item.Title,
        item.Description,
        item.MediaType,
        item.PosterUrl,
        item.CommunityRating,
        item.ReleaseDate,
        string.IsNullOrWhiteSpace(item.CastLabel)
            ? []
            : item.CastLabel.Split(", ", StringSplitOptions.RemoveEmptyEntries));
}
