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
    public void Update_WithValidRating_UpdatesEditableFields()
    {
        var item = MediaItem.Create("Dune", MediaType.Movie);

        item.Update("Dune: Part Two", MediaType.Movie, "Updated", MediaStatus.Completed, 9m);

        Assert.Equal("Dune: Part Two", item.Title);
        Assert.Equal(MediaStatus.Completed, item.Status);
        Assert.Equal(9m, item.PersonalRating);
    }

    [Theory]
    [InlineData(-0.5)]
    [InlineData(10.5)]
    public void Update_WithRatingOutsideRange_Throws(decimal rating)
    {
        var item = MediaItem.Create("Dune", MediaType.Movie);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            item.Update("Dune", MediaType.Movie, null, MediaStatus.Planned, rating));
    }
}
