using Nookly.Contracts.Media;
using Nookly.Contracts.Search;
using Nookly.Desktop.Services;
using Nookly.Desktop.ViewModels;

namespace Nookly.Desktop.Tests.ViewModels;

public sealed class LibraryViewModelTests
{
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

    [Fact]
    public async Task SearchAndAddResult_AddsExternalMediaToLibrary()
    {
        var apiClient = new StubMediaApiClient
        {
            SearchResults =
            [
                new MediaSearchResultResponse(
                    "tmdb", "438631", "Dune", "Science fiction", MediaType.Movie,
                    "https://image.tmdb.org/t/p/w500/poster.jpg", 7.8m,
                    new DateOnly(2021, 9, 15))
            ]
        };
        var viewModel = CreateViewModel(apiClient);
        viewModel.SearchQuery = "Dune";

        viewModel.OpenDiscoverCommand.Execute(null);
        await viewModel.SearchCommand.ExecuteAsync(null);
        var searchResult = Assert.Single(viewModel.SearchResults);
        viewModel.ShowSearchResultCommand.Execute(searchResult);

        Assert.True(viewModel.IsDetailPage);
        Assert.Equal("Dune", viewModel.SelectedSearchResult?.Title);

        await viewModel.AddSearchResultCommand.ExecuteAsync(searchResult);

        Assert.Contains(viewModel.Items, item => item.Title == "Dune");
        Assert.True(viewModel.IsLibraryPage);
        Assert.NotNull(apiClient.LastCreateRequest);
        Assert.Equal("tmdb", apiClient.LastCreateRequest.ExternalSource);
        Assert.Equal("438631", apiClient.LastCreateRequest.ExternalId);
    }

    [Fact]
    public async Task LibrarySearch_FiltersOnlyExistingItems()
    {
        var viewModel = CreateViewModel(new StubMediaApiClient());
        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.LibrarySearchQuery = "Dun";
        Assert.Single(viewModel.FilteredItems);

        viewModel.LibrarySearchQuery = "Bleach";
        Assert.Empty(viewModel.FilteredItems);
        Assert.True(viewModel.ShowNoLibraryResults);
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
        public IReadOnlyList<MediaSearchResultResponse> SearchResults { get; init; } = [];
        public CreateMediaRequest? LastCreateRequest { get; private set; }

        public Task<IReadOnlyList<MediaItemResponse>> GetMediaAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<MediaItemResponse>>([item]);
        }

        public Task<IReadOnlyList<MediaSearchResultResponse>> SearchMediaAsync(
            string query,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(SearchResults);
        }

        public Task<MediaItemResponse> CreateMediaAsync(
            CreateMediaRequest request,
            CancellationToken cancellationToken = default)
        {
            LastCreateRequest = request;
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
                null,
                null,
                null,
                null,
                null,
                now,
                now);
        }
    }

    private sealed class ConfirmingDialogService : IUserDialogService
    {
        public bool ConfirmDelete(string title) => true;
    }
}
