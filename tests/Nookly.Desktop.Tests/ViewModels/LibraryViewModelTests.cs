using Nookly.Contracts.Media;
using Nookly.Contracts.Search;
using Nookly.Contracts.Discovery;
using Nookly.Contracts.Details;
using Nookly.Contracts.Banking;
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
        await viewModel.ShowSearchResultCommand.ExecuteAsync(searchResult);

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
    public async Task Discover_HidesMediaAlreadyInLibrary()
    {
        var existing = StubMediaApiClient.CreateResponse(
            "Dune", MediaType.Movie, MediaStatus.Planned, null,
            externalSource: "tmdb", externalId: "438631");
        var apiClient = new StubMediaApiClient
        {
            MediaItems = [existing],
            SearchResults =
            [
                new MediaSearchResultResponse(
                    "tmdb", "438631", "Dune", null, MediaType.Movie,
                    null, 7.8m, new DateOnly(2021, 9, 15)),
                new MediaSearchResultResponse(
                    "tmdb", "693134", "Dune : Deuxieme partie", null, MediaType.Movie,
                    null, 8.1m, new DateOnly(2024, 2, 28))
            ]
        };
        var viewModel = CreateViewModel(apiClient);

        await viewModel.LoadCommand.ExecuteAsync(null);
        await viewModel.OpenDiscoverCommand.ExecuteAsync(null);

        var result = Assert.Single(viewModel.SearchResults);
        Assert.Equal("693134", result.Result.ExternalId);
    }

    [Fact]
    public async Task DiscoveryHistory_DisplaysMostRecentItemsFromApi()
    {
        var apiClient = new StubMediaApiClient
        {
            DiscoveryHistory =
            [
                new DiscoveryHistoryResponse(
                    "tmdb", "693134", MediaType.Movie, "Dune : Deuxieme partie",
                    null, null, 8.1m, new DateOnly(2024, 2, 28), null,
                    new DateTimeOffset(2026, 9, 19, 10, 0, 0, TimeSpan.Zero)),
                new DiscoveryHistoryResponse(
                    "tmdb", "438631", MediaType.Movie, "Dune",
                    null, null, 7.8m, new DateOnly(2021, 9, 15), null,
                    new DateTimeOffset(2026, 9, 18, 10, 0, 0, TimeSpan.Zero))
            ]
        };
        var viewModel = CreateViewModel(apiClient);

        await viewModel.ShowDiscoveryHistoryCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsDiscoveryHistorySelected);
        Assert.Equal("Dune : Deuxieme partie", viewModel.DiscoveryHistory[0].Title);
        Assert.Equal("Dune", viewModel.DiscoveryHistory[1].Title);
    }

    [Fact]
    public async Task DiscoveryFilters_AllowMultipleTypesGenresAndCountries()
    {
        var apiClient = new StubMediaApiClient();
        var viewModel = CreateViewModel(apiClient);

        await viewModel.ToggleDiscoveryTypeCommand.ExecuteAsync(viewModel.DiscoveryMediaTypes[0]);
        await viewModel.ToggleDiscoveryTypeCommand.ExecuteAsync(viewModel.DiscoveryMediaTypes[1]);
        await viewModel.ToggleDiscoveryGenreCommand.ExecuteAsync(
            viewModel.DiscoveryGenres.Single(item => item.Label == "Thriller"));
        await viewModel.ToggleDiscoveryGenreCommand.ExecuteAsync(
            viewModel.DiscoveryGenres.Single(item => item.Label == "Drame"));
        await viewModel.ToggleDiscoveryCountryCommand.ExecuteAsync(
            viewModel.DiscoveryCountries.Single(item => item.Code == "FR"));

        Assert.Equal([MediaType.Movie, MediaType.TvSeries], apiClient.LastSearchTypes);
        Assert.Equal([18, 53], apiClient.LastSearchGenreIds);
        Assert.Equal(["FR"], apiClient.LastSearchCountries);
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

    [Fact]
    public async Task DislikeDiscoveryResult_RemovesAndPersistsTheResult()
    {
        var apiClient = new StubMediaApiClient
        {
            SearchResults =
            [
                new MediaSearchResultResponse(
                    "tmdb", "438631", "Dune", null, MediaType.Movie,
                    null, 7.8m, new DateOnly(2021, 9, 15))
            ]
        };
        var viewModel = CreateViewModel(apiClient);
        await viewModel.OpenDiscoverCommand.ExecuteAsync(null);
        var result = Assert.Single(viewModel.SearchResults);

        await viewModel.DislikeDiscoveryResultCommand.ExecuteAsync(result);

        Assert.Empty(viewModel.SearchResults);
        Assert.NotNull(apiClient.LastPreferenceRequest);
        Assert.False(apiClient.LastPreferenceRequest.IsLiked);
        Assert.Equal("438631", apiClient.LastPreferenceRequest.ExternalId);
    }

    [Fact]
    public async Task Settings_CanRestoreAnAccidentallyDislikedTitle()
    {
        var apiClient = new StubMediaApiClient
        {
            DislikedPreferences =
            [
                new DiscoveryPreferenceResponse("tmdb", "438631", "Dune", MediaType.Movie, false)
            ]
        };
        var viewModel = CreateViewModel(apiClient);

        await viewModel.ShowSettingsCommand.ExecuteAsync(null);
        var preference = Assert.Single(viewModel.DislikedPreferences);
        await viewModel.RestoreDiscoveryPreferenceCommand.ExecuteAsync(preference);

        Assert.True(viewModel.IsSettingsPage);
        Assert.Empty(viewModel.DislikedPreferences);
        Assert.False(viewModel.HasDislikedPreferences);
        Assert.True(apiClient.RestorePreferenceCalled);
    }

    [Fact]
    public async Task Settings_SearchFiltersHiddenTitlesByTitleOrType()
    {
        var apiClient = new StubMediaApiClient
        {
            DislikedPreferences =
            [
                new DiscoveryPreferenceResponse("tmdb", "438631", "Dune", MediaType.Movie, false),
                new DiscoveryPreferenceResponse("tmdb", "30984", "Bleach", MediaType.Anime, false)
            ]
        };
        var viewModel = CreateViewModel(apiClient);
        await viewModel.ShowSettingsCommand.ExecuteAsync(null);

        viewModel.DislikedPreferencesSearchQuery = "Ble";
        Assert.Equal("Bleach", Assert.Single(viewModel.FilteredDislikedPreferences).Title);

        viewModel.DislikedPreferencesSearchQuery = "Film";
        Assert.Equal("Dune", Assert.Single(viewModel.FilteredDislikedPreferences).Title);

        viewModel.DislikedPreferencesSearchQuery = "inconnu";
        Assert.Empty(viewModel.FilteredDislikedPreferences);
        Assert.True(viewModel.ShowNoFilteredDislikedPreferences);
    }

    [Fact]
    public async Task SeriesDetails_LoadFirstSeasonAndEpisodes()
    {
        var apiClient = new StubMediaApiClient
        {
            SearchResults =
            [
                new MediaSearchResultResponse(
                    "tmdb", "30984", "Bleach", null, MediaType.Anime,
                    null, 8.4m, new DateOnly(2004, 10, 5))
            ],
            Details = StubMediaApiClient.CreateSeriesDetails()
        };
        var viewModel = CreateViewModel(apiClient);
        await viewModel.OpenDiscoverCommand.ExecuteAsync(null);

        await viewModel.ShowSearchResultCommand.ExecuteAsync(Assert.Single(viewModel.SearchResults));
        await WaitUntilAsync(() => viewModel.SelectedSeasonDetails is not null, TimeSpan.FromSeconds(2));

        Assert.Equal("Bleach", viewModel.SelectedMediaDetails?.Title);
        Assert.Equal("Saison 1", viewModel.SelectedSeasonDetails?.Title);
        Assert.Equal("Le jour ou je suis devenu un Shinigami",
            Assert.Single(viewModel.SelectedSeasonDetails!.Episodes).Title);
    }

    [Fact]
    public async Task EpisodeProgress_IsSavedAndUpdatesSeasonCount()
    {
        var apiClient = new StubMediaApiClient
        {
            SearchResults =
            [
                new MediaSearchResultResponse(
                    "tmdb", "30984", "Bleach", null, MediaType.Anime,
                    null, 8.4m, new DateOnly(2004, 10, 5))
            ],
            Details = StubMediaApiClient.CreateSeriesDetails()
        };
        var viewModel = CreateViewModel(apiClient);
        await viewModel.OpenDiscoverCommand.ExecuteAsync(null);
        await viewModel.ShowSearchResultCommand.ExecuteAsync(Assert.Single(viewModel.SearchResults));
        await WaitUntilAsync(() => viewModel.SelectedSeasonDetails is not null, TimeSpan.FromSeconds(2));
        var episode = Assert.Single(viewModel.SelectedSeasonDetails!.Episodes);
        episode.IsWatched = true;
        episode.PersonalNotes = "Tres bon debut";

        await viewModel.SaveEpisodeProgressCommand.ExecuteAsync(episode);

        Assert.True(apiClient.LastEpisodeProgressRequest?.IsWatched);
        Assert.Equal("Tres bon debut", apiClient.LastEpisodeProgressRequest?.PersonalNotes);
        Assert.Equal("1 / 1 episodes vus", viewModel.SelectedSeasonDetails.ProgressLabel);
    }

    [Fact]
    public async Task ResetBankMonthCommand_ResetsAndReloadsTheCurrentPeriod()
    {
        var bankApiClient = new StubBankApiClient();
        var viewModel = new LibraryViewModel(
            new StubMediaApiClient(),
            new ConfirmingDialogService(),
            bankApiClient);

        await viewModel.ResetBankMonthCommand.ExecuteAsync(null);

        Assert.True(bankApiClient.ResetCalled);
        Assert.True(bankApiClient.GetCallCount > 0);
    }

    private static LibraryViewModel CreateViewModel(StubMediaApiClient apiClient)
    {
        return new LibraryViewModel(apiClient, new ConfirmingDialogService(), new StubBankApiClient());
    }

    private sealed class StubMediaApiClient : IMediaApiClient
    {
        private MediaItemResponse item = CreateResponse(
            "Dune",
            MediaType.Movie,
            MediaStatus.Planned,
            null);

        public bool DeleteCalled { get; private set; }
        public IReadOnlyList<MediaItemResponse>? MediaItems { get; init; }
        public IReadOnlyList<MediaSearchResultResponse> SearchResults { get; init; } = [];
        public Func<string, CancellationToken, Task<IReadOnlyList<MediaSearchResultResponse>>>?
            SearchHandler
        { get; init; }
        public CreateMediaRequest? LastCreateRequest { get; private set; }
        public SetDiscoveryPreferenceRequest? LastPreferenceRequest { get; private set; }
        public IReadOnlyList<DiscoveryPreferenceResponse> DislikedPreferences { get; init; } = [];
        public IReadOnlyList<DiscoveryHistoryResponse> DiscoveryHistory { get; init; } = [];
        public IReadOnlyCollection<MediaType> LastSearchTypes { get; private set; } = [];
        public IReadOnlyCollection<int> LastSearchGenreIds { get; private set; } = [];
        public IReadOnlyCollection<string> LastSearchCountries { get; private set; } = [];
        public bool RestorePreferenceCalled { get; private set; }
        public MediaDetailsResponse? Details { get; init; }
        public UpdateSeasonProgressRequest? LastSeasonProgressRequest { get; private set; }
        public UpdateEpisodeProgressRequest? LastEpisodeProgressRequest { get; private set; }

        public Task<IReadOnlyList<MediaItemResponse>> GetMediaAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(MediaItems ?? [item]);
        }

        public Task<IReadOnlyList<MediaSearchResultResponse>> SearchMediaAsync(
            string? query,
            IReadOnlyCollection<MediaType>? types = null,
            IReadOnlyCollection<int>? genreIds = null,
            int? year = null,
            string? actor = null,
            IReadOnlyCollection<string>? countries = null,
            CancellationToken cancellationToken = default)
        {
            LastSearchTypes = types ?? [];
            LastSearchGenreIds = genreIds ?? [];
            LastSearchCountries = countries ?? [];
            return SearchHandler?.Invoke(query ?? string.Empty, cancellationToken)
                   ?? Task.FromResult(SearchResults);
        }

        public Task<MediaDetailsResponse> GetMediaDetailsAsync(
            string externalId,
            MediaType type,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Details ?? new MediaDetailsResponse(
                externalId, "Dune", type, null, null, null, null, "Sorti", 155,
                8m, [], [], [], null, []));

        public Task<SeasonDetailsResponse> GetSeasonDetailsAsync(
            string externalId,
            int seasonNumber,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new SeasonDetailsResponse(
                seasonNumber,
                $"Saison {seasonNumber}",
                "Premiere saison",
                null,
                new DateOnly(2004, 10, 5),
                8.2m,
                [new EpisodeResponse(1, "Le jour ou je suis devenu un Shinigami", "Ichigo rencontre Rukia.", null, new DateOnly(2004, 10, 5), 24, 8.1m)]));

        public Task<SeasonProgressResponse> GetSeasonProgressAsync(
            string externalId,
            int seasonNumber,
            int totalEpisodes,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new SeasonProgressResponse(seasonNumber, null, 0, totalEpisodes, []));

        public Task UpdateSeasonProgressAsync(
            string externalId,
            int seasonNumber,
            UpdateSeasonProgressRequest request,
            CancellationToken cancellationToken = default)
        {
            LastSeasonProgressRequest = request;
            return Task.CompletedTask;
        }

        public Task UpdateEpisodeProgressAsync(
            string externalId,
            int seasonNumber,
            int episodeNumber,
            UpdateEpisodeProgressRequest request,
            CancellationToken cancellationToken = default)
        {
            LastEpisodeProgressRequest = request;
            return Task.CompletedTask;
        }

        public Task<MediaItemResponse> CreateMediaAsync(
            CreateMediaRequest request,
            CancellationToken cancellationToken = default)
        {
            LastCreateRequest = request;
            item = CreateResponse(
                request.Title,
                request.Type,
                MediaStatus.Planned,
                null,
                externalSource: request.ExternalSource,
                externalId: request.ExternalId);
            return Task.FromResult(item);
        }

        public Task SetDiscoveryPreferenceAsync(
            SetDiscoveryPreferenceRequest request,
            CancellationToken cancellationToken = default)
        {
            LastPreferenceRequest = request;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<DiscoveryPreferenceResponse>> GetDislikedPreferencesAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(DislikedPreferences);

        public Task<IReadOnlyList<DiscoveryHistoryResponse>> GetDiscoveryHistoryAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(DiscoveryHistory);

        public Task RestoreDiscoveryPreferenceAsync(
            string source,
            string externalId,
            CancellationToken cancellationToken = default)
        {
            RestorePreferenceCalled = true;
            return Task.CompletedTask;
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

        public static MediaItemResponse CreateResponse(
            string title,
            MediaType type,
            MediaStatus status,
            decimal? rating,
            Guid? id = null,
            string? personalNotes = null,
            bool isFavorite = false,
            int? currentSeason = null,
            int? currentEpisode = null,
            string? externalSource = null,
            string? externalId = null)
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
                externalSource,
                externalId,
                null,
                null,
                null,
                now,
                now);
        }

        public static MediaDetailsResponse CreateSeriesDetails() => new(
            "30984",
            "Bleach",
            MediaType.Anime,
            "Ichigo devient Shinigami.",
            null,
            null,
            new DateOnly(2004, 10, 5),
            "Terminee",
            24,
            8.4m,
            ["Animation", "Action"],
            ["Noriyuki Abe"],
            ["Masakazu Morita"],
            null,
            [new SeasonSummaryResponse(1, "Saison 1", "Premiere saison", null, new DateOnly(2004, 10, 5), 20, 8.2m)]);
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
        public bool ConfirmBankMonthReset() => true;
    }

    private sealed class StubBankApiClient : IBankApiClient
    {
        public bool ResetCalled { get; private set; }
        public int GetCallCount { get; private set; }

        public Task<BankSummaryResponse> GetAsync(CancellationToken token = default)
        {
            GetCallCount++;
            return Task.FromResult(new BankSummaryResponse(0, 0, []));
        }
        public Task<BankSummaryResponse> SetStartingBalanceAsync(decimal amount, CancellationToken token = default) =>
            Task.FromResult(new BankSummaryResponse(amount, amount, []));
        public Task<BankEntryResponse> AddEntryAsync(string label, decimal amount, CancellationToken token = default) =>
            Task.FromResult(new BankEntryResponse(Guid.NewGuid(), label, amount, DateTimeOffset.UtcNow));
        public Task DeleteEntryAsync(Guid id, CancellationToken token = default) => Task.CompletedTask;
        public Task ResetCurrentMonthAsync(CancellationToken token = default)
        {
            ResetCalled = true;
            return Task.CompletedTask;
        }
    }
}
