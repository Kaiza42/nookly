using Nookly.Contracts.Media;
using Nookly.Contracts.Search;

namespace Nookly.Desktop.ViewModels;

public sealed class MediaSearchResultViewModel(MediaSearchResultResponse result)
{
    public MediaSearchResultResponse Result => result;
    public string Title => result.Title;
    public string? Description => result.Description;
    public string? PosterUrl => result.PosterUrl;
    public string TypeLabel => result.Type switch
    {
        MediaType.Movie => "Film",
        MediaType.TvSeries => "Serie",
        MediaType.Anime => "Anime",
        _ => result.Type.ToString()
    };
    public string YearLabel => result.ReleaseDate?.Year.ToString() ?? "Date inconnue";
    public string RatingLabel => result.CommunityRating is null
        ? "Non note"
        : $"{result.CommunityRating:0.0}/10";
}
