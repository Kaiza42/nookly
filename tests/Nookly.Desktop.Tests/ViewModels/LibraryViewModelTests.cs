using Nookly.Contracts.Media;
using Nookly.Contracts.Search;
using Nookly.Desktop.Services;
using Nookly.Desktop.ViewModels;

namespace Nookly.Desktop.Tests.ViewModels;

public sealed class LibraryViewModelTests
{
    [Fact]
    public async Task SaveMediaCommand_UpdatesOnlyPersonalTracking()
    {
        var apiClient = new StubMediaApiClient();
        var viewModel = CreateViewModel(apiClient);
        await viewModel.LoadCommand.ExecuteAsync(null);
        var item = Assert.Single(viewModel.Items);
        viewModel.EditMediaCommand.Execute(item);
        Assert.True(viewModel.SaveMediaCommand.CanExecute(null));
        viewModel.PersonalRatingText = "9";
        viewModel.NewPersonalNotes = "Mon avis personnel";
        viewModel.IsFavorite = true;

        await viewModel.SaveMediaCommand.ExecuteAsync(null);

        var updated = Assert.Single(viewModel.Items);
        Assert.Equal("Dune", updated.Title);
        Assert.Equal(9m, updated.PersonalRating);
        Assert.Equal("Mon avis personnel", updated.PersonalNotes);
        Assert.True(updated.IsFavorite);
    }

    [Fact]
    public async Task SaveMediaCommand_AcceptsCommaDecimalRating()
    {
        var viewModel = CreateViewModel(new StubMediaApiClient());
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.EditMediaCommand.Execute(Assert.Single(viewModel.Items));
        viewModel.PersonalRatingText = "8,75";

        await viewModel.SaveMediaCommand.ExecuteAsync(null);

        Assert.Equal(8.75m, Assert.Single(viewModel.Items).PersonalRating);
        Assert.False(viewModel.HasFormError);
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

        await viewModel.OpenDiscoverCommand.ExecuteAsync(null);
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

    [Fact]
    public async Task CatalogSearch_CanSearchAgainAfterClearingAnInFlightQuery()
    {
        var firstSearchStarted = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var apiClient = new StubMediaApiClient
        {
            SearchHandler = async (query, cancellationToken) =>
            {
                if (query == "Bleach")
                {
                    firstSearchStarted.SetResult();
                    await Task.Delay(Timeout.Infinite, cancellationToken);
                }

                return
                [
                    new MediaSearchResultResponse(
                        "tmdb", "438631", "Dune", null, MediaType.Movie,
                        null, 7.8m, new DateOnly(2021, 9, 15))
                ];
            }
        };
        var viewModel = CreateViewModel(apiClient);

        viewModel.SearchQuery = "Bleach";
        await firstSearchStarted.Task.WaitAsync(TimeSpan.FromSeconds(2));
        viewModel.SearchQuery = string.Empty;
        viewModel.SearchQuery = "Dune";

        await WaitUntilAsync(() => viewModel.HasSearchResults, TimeSpan.FromSeconds(2));

        Assert.False(viewModel.IsSearching);
        Assert.Equal("Dune", Assert.Single(viewModel.SearchResults).Title);
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
        public Func<string, CancellationToken, Task<IReadOnlyList<MediaSearchResultResponse>>>?
            SearchHandler
        { get; init; }
        public CreateMediaRequest? LastCreateRequest { get; private set; }

        public Task<IReadOnlyList<MediaItemResponse>> GetMediaAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<MediaItemResponse>>([item]);
        }

        public Task<IReadOnlyList<MediaSearchResultResponse>> SearchMediaAsync(
            string? query,
            MediaType? type = null,
            int? genreId = null,
            int? year = null,
            string? actor = null,
            CancellationToken cancellationToken = default)
        {
            return SearchHandler?.Invoke(query ?? string.Empty, cancellationToken)
                   ?? Task.FromResult(SearchResults);
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
            item = CreateResponse(
                item.Title,
                item.Type,
                request.Status,
                request.PersonalRating,
                id,
                request.PersonalNotes,
                request.IsFavorite,
                request.CurrentSeason,
                request.CurrentEpisode);
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
            Guid? id = null,
            string? personalNotes = null,
            bool isFavorite = false,
            int? currentSeason = null,
            int? currentEpisode = null)
        {
            var now = DateTimeOffset.UtcNow;
            return new MediaItemResponse(
                id ?? Guid.NewGuid(),
                title,
                null,
                type,
                status,
                rating,
                personalNotes,
                isFavorite,
                currentSeason,
                currentEpisode,
                null,
                null,
                null,
                null,
                null,
                now,
                now);
        }
    }

    private static async Task WaitUntilAsync(Func<bool> condition, TimeSpan timeout)
    {
        using var cancellation = new CancellationTokenSource(timeout);
        while (!condition())
        {
            await Task.Delay(20, cancellation.Token);
        }
    }

    private sealed class ConfirmingDialogService : IUserDialogService
    {
        public bool ConfirmDelete(string title) => true;
    }
}
