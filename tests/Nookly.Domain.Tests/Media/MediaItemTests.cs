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
}
