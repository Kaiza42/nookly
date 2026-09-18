using Nookly.Domain.Media;

namespace Nookly.Domain.Tests.Media;

public sealed class EpisodeProgressTests
{
    [Fact]
    public void Update_PreservesPersonalTracking()
    {
        var progress = EpisodeProgress.Create(Guid.NewGuid(), "TMDB", "30984", 1, 3);

        progress.Update(true, "Excellent episode");

        Assert.True(progress.IsWatched);
        Assert.Equal("Excellent episode", progress.PersonalNotes);
        Assert.Equal("tmdb", progress.ExternalSource);
    }

    [Fact]
    public void Create_RejectsInvalidNumbers()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            EpisodeProgress.Create(Guid.NewGuid(), "tmdb", "30984", 0, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            EpisodeProgress.Create(Guid.NewGuid(), "tmdb", "30984", 1, 0));
    }

    [Fact]
    public void SeasonNotes_CanBeCleared()
    {
        var progress = SeasonProgress.Create(Guid.NewGuid(), "tmdb", "30984", 1);
        progress.SetNotes("Une bonne saison");

        progress.SetNotes("  ");

        Assert.Null(progress.PersonalNotes);
    }
}
