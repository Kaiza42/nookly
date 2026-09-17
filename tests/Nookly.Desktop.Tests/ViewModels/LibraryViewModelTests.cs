using Nookly.Contracts.Media;
using Nookly.Desktop.Services;
using Nookly.Desktop.ViewModels;

namespace Nookly.Desktop.Tests.ViewModels;

public sealed class LibraryViewModelTests
{
    [Fact]
    public async Task SaveMediaCommand_AddsCreatedMediaAndClosesForm()
    {
        var apiClient = new StubMediaApiClient();
        var viewModel = CreateViewModel(apiClient);
        viewModel.ToggleCreatePanelCommand.Execute(null);
        viewModel.NewTitle = "Berserk";
        viewModel.NewDescription = "Manga";
        viewModel.SelectedMediaType = viewModel.MediaTypes.Single(x => x.Value == MediaType.Manga);

        await viewModel.SaveMediaCommand.ExecuteAsync(null);

        var item = Assert.Single(viewModel.Items);
        Assert.Equal("Berserk", item.Title);
        Assert.True(viewModel.HasItems);
        Assert.False(viewModel.IsCreatePanelOpen);
    }

    [Fact]
    public async Task SaveMediaCommand_UpdatesSelectedMediaAndRating()
    {
        var apiClient = new StubMediaApiClient();
        var viewModel = CreateViewModel(apiClient);
        await viewModel.LoadCommand.ExecuteAsync(null);
        var item = Assert.Single(viewModel.Items);
        viewModel.EditMediaCommand.Execute(item);
        viewModel.NewTitle = "Dune: Part Two";
        viewModel.SelectedRating = viewModel.Ratings.Single(x => x.Value == 9m);

        await viewModel.SaveMediaCommand.ExecuteAsync(null);

        var updated = Assert.Single(viewModel.Items);
        Assert.Equal("Dune: Part Two", updated.Title);
        Assert.Equal(9m, updated.PersonalRating);
    }

    [Fact]
    public async Task DeleteMediaCommand_RemovesConfirmedMedia()
    {
        var apiClient = new StubMediaApiClient();
        var viewModel = CreateViewModel(apiClient);
        await viewModel.LoadCommand.ExecuteAsync(null);

        await viewModel.DeleteMediaCommand.ExecuteAsync(Assert.Single(viewModel.Items));

        Assert.Empty(viewModel.Items);
        Assert.False(viewModel.HasItems);
        Assert.True(apiClient.DeleteCalled);
    }

    private static LibraryViewModel CreateViewModel(StubMediaApiClient apiClient)
    {
        return new LibraryViewModel(apiClient, new ConfirmingDialogService());
    }

    private sealed class StubMediaApiClient : IMediaApiClient
    {
        private MediaItemResponse item = CreateResponse(
            "Dune",
            MediaType.Movie,
            MediaStatus.Planned,
            null);

        public bool DeleteCalled { get; private set; }

        public Task<IReadOnlyList<MediaItemResponse>> GetMediaAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<MediaItemResponse>>([item]);
        }

        public Task<MediaItemResponse> CreateMediaAsync(
            CreateMediaRequest request,
            CancellationToken cancellationToken = default)
        {
            item = CreateResponse(request.Title, request.Type, MediaStatus.Planned, null);
            return Task.FromResult(item);
        }

        public Task<MediaItemResponse> UpdateMediaAsync(
            Guid id,
            UpdateMediaRequest request,
            CancellationToken cancellationToken = default)
        {
            item = CreateResponse(request.Title, request.Type, request.Status, request.PersonalRating, id);
            return Task.FromResult(item);
        }

        public Task DeleteMediaAsync(Guid id, CancellationToken cancellationToken = default)
        {
            DeleteCalled = true;
            return Task.CompletedTask;
        }

        private static MediaItemResponse CreateResponse(
            string title,
            MediaType type,
            MediaStatus status,
            decimal? rating,
            Guid? id = null)
        {
            var now = DateTimeOffset.UtcNow;
            return new MediaItemResponse(
                id ?? Guid.NewGuid(),
                title,
                null,
                type,
                status,
                rating,
                now,
                now);
        }
    }

    private sealed class ConfirmingDialogService : IUserDialogService
    {
        public bool ConfirmDelete(string title) => true;
    }
}
