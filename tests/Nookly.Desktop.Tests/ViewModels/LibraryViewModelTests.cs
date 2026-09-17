using Nookly.Contracts.Media;
using Nookly.Desktop.Services;
using Nookly.Desktop.ViewModels;

namespace Nookly.Desktop.Tests.ViewModels;

public sealed class LibraryViewModelTests
{
    [Fact]
    public async Task AddMediaCommand_AddsCreatedMediaAndClosesForm()
    {
        var apiClient = new StubMediaApiClient();
        var viewModel = new LibraryViewModel(apiClient)
        {
            IsCreatePanelOpen = true,
            NewTitle = "Berserk",
            NewDescription = "Manga",
            SelectedMediaType = new MediaTypeOption("Manga", MediaType.Manga)
        };

        await viewModel.AddMediaCommand.ExecuteAsync(null);

        var item = Assert.Single(viewModel.Items);
        Assert.Equal("Berserk", item.Title);
        Assert.True(viewModel.HasItems);
        Assert.False(viewModel.IsCreatePanelOpen);
        Assert.Equal(string.Empty, viewModel.NewTitle);
    }

    private sealed class StubMediaApiClient : IMediaApiClient
    {
        public Task<IReadOnlyList<MediaItemResponse>> GetMediaAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<MediaItemResponse>>([]);
        }

        public Task<MediaItemResponse> CreateMediaAsync(
            CreateMediaRequest request,
            CancellationToken cancellationToken = default)
        {
            var response = new MediaItemResponse(
                Guid.NewGuid(),
                request.Title,
                request.Description,
                request.Type,
                MediaStatus.Planned,
                DateTimeOffset.UtcNow);

            return Task.FromResult(response);
        }
    }
}
