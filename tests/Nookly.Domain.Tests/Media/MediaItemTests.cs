using Nookly.Domain.Media;

namespace Nookly.Domain.Tests.Media;

public sealed class MediaItemTests
{
    [Fact]
    public void Create_WithValidTitle_CreatesPlannedMedia()
    {
        var item = MediaItem.Create("  Dune  ", MediaType.Movie, "Science fiction");

        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.Equal("Dune", item.Title);
        Assert.Equal(MediaType.Movie, item.Type);
        Assert.Equal(MediaStatus.Planned, item.Status);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutTitle_ThrowsArgumentException(string title)
    {
        Assert.Throws<ArgumentException>(() => MediaItem.Create(title, MediaType.Movie));
    }

    [Fact]
    public void UpdatePersonalTracking_UpdatesOnlyPersonalFields()
    {
        var item = MediaItem.Create("Dune", MediaType.Movie, "Science fiction");

        item.UpdatePersonalTracking(MediaStatus.Completed, 9m, "Excellent film");

        Assert.Equal("Dune", item.Title);
        Assert.Equal(MediaType.Movie, item.Type);
        Assert.Equal("Science fiction", item.Description);
        Assert.Equal(MediaStatus.Completed, item.Status);
        Assert.Equal(9m, item.PersonalRating);
        Assert.Equal("Excellent film", item.PersonalNotes);
    }

    [Theory]
    [InlineData(-0.5)]
    [InlineData(10.5)]
    public void Update_WithRatingOutsideRange_Throws(decimal rating)
    {
        var item = MediaItem.Create("Dune", MediaType.Movie);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            item.UpdatePersonalTracking(MediaStatus.Planned, rating, null));
    }

    [Fact]
    public void UpdatePersonalTracking_ForSeries_SavesFavoriteAndProgress()
    {
        var item = MediaItem.Create("Bleach", MediaType.Anime);

        item.UpdatePersonalTracking(MediaStatus.InProgress, 8.75m, "A continuer", true, 2, 14);

        Assert.True(item.IsFavorite);
        Assert.Equal(2, item.CurrentSeason);
        Assert.Equal(14, item.CurrentEpisode);
    }

    [Fact]
    public void UpdatePersonalTracking_ForMovie_DiscardsEpisodeProgress()
    {
        var item = MediaItem.Create("Dune", MediaType.Movie);

        item.UpdatePersonalTracking(MediaStatus.Completed, 8m, null, false, 3, 12);

        Assert.Null(item.CurrentSeason);
        Assert.Null(item.CurrentEpisode);
    }
}
