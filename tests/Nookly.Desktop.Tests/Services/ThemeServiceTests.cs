using Nookly.Desktop.Services;

namespace Nookly.Desktop.Tests.Services;

public sealed class ThemeServiceTests
{
    [Theory]
    [InlineData("#286C53")]
    [InlineData("#FFF")]
    [InlineData("Red")]
    public void IsValidColor_AcceptsWpfColors(string value) =>
        Assert.True(ThemeService.IsValidColor(value));

    [Theory]
    [InlineData("")]
    [InlineData("286C53")]
    [InlineData("not-a-color")]
    public void IsValidColor_RejectsInvalidValues(string value) =>
        Assert.False(ThemeService.IsValidColor(value));

    [Fact]
    public void Presets_ExposeTheExpectedThemeColors()
    {
        Assert.True(ThemeService.Presets.Count >= 5);
        Assert.All(ThemeService.Presets.Values, palette =>
        {
            Assert.True(ThemeService.IsValidColor(palette.Background));
            Assert.True(ThemeService.IsValidColor(palette.Surface));
            Assert.True(ThemeService.IsValidColor(palette.Sidebar));
            Assert.True(ThemeService.IsValidColor(palette.Accent));
            Assert.True(ThemeService.IsValidColor(palette.Text));
            Assert.True(ThemeService.IsValidColor(palette.MutedText));
            Assert.True(ThemeService.IsValidColor(palette.Border));
            Assert.True(ThemeService.IsValidColor(palette.TitleBar!));
            Assert.Equal(palette.Background, palette.TitleBar);
        });
    }
}
