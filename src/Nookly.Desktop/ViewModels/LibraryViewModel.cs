using System.Collections.ObjectModel;
using System.Net.Http;
using System.Globalization;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nookly.Contracts.Media;
using Nookly.Contracts.Discovery;
using Nookly.Contracts.Details;
using Nookly.Contracts.Search;
using Nookly.Desktop.Services;

namespace Nookly.Desktop.ViewModels;

public partial class LibraryViewModel(
    IMediaApiClient mediaApiClient,
    IUserDialogService userDialogService,
    IBankApiClient bankApiClient) : ObservableObject
{
    private Guid? editingMediaId;
    private CancellationTokenSource? searchDebounceCancellation;
    private int searchVersion;
    private CancellationTokenSource? detailsCancellation;

    public ObservableCollection<MediaListItemViewModel> Items { get; } = [];
    public ObservableCollection<MediaListItemViewModel> FilteredItems { get; } = [];
    public ObservableCollection<MediaSearchResultViewModel> SearchResults { get; } = [];
    public ObservableCollection<DiscoveryHistoryViewModel> DiscoveryHistory { get; } = [];
    public ObservableCollection<DiscoveryPreferenceViewModel> DislikedPreferences { get; } = [];
    public ObservableCollection<DiscoveryPreferenceViewModel> FilteredDislikedPreferences { get; } = [];
    public ObservableCollection<BankEntryViewModel> BankEntries { get; } = [];
    public ObservableCollection<BankMonthViewModel> BankHistory { get; } = [];

    public IReadOnlyList<MediaTypeOption> MediaTypes { get; } =
    [
        new("Film", MediaType.Movie),
        new("Serie", MediaType.TvSeries),
        new("Anime", MediaType.Anime),
        new("Manga", MediaType.Manga)
    ];

    public IReadOnlyList<MediaStatusOption> MediaStatuses { get; } =
    [
        new("A decouvrir", MediaStatus.Planned),
        new("En cours", MediaStatus.InProgress),
        new("Termine", MediaStatus.Completed),
        new("En pause", MediaStatus.OnHold),
        new("Abandonne", MediaStatus.Dropped)
    ];

    public IReadOnlyList<DiscoveryTypeOption> DiscoveryMediaTypes { get; } =
    [
        new("Films", MediaType.Movie),
        new("Series", MediaType.TvSeries),
        new("Animes", MediaType.Anime)
    ];

    public IReadOnlyList<GenreOption> DiscoveryGenres { get; } =
    [
        new("Action", 28),
        new("Animation", 16),
        new("Aventure", 12),
        new("Comedie", 35),
        new("Crime", 80),
        new("Documentaire", 99),
        new("Drame", 18),
        new("Fantastique", 14),
        new("Horreur", 27),
        new("Romance", 10749),
        new("Science-fiction", 878),
        new("Thriller", 53)
    ];

    public IReadOnlyList<CountryOption> DiscoveryCountries { get; } =
    [
        new("France", "FR"),
        new("Etats-Unis", "US"),
        new("Japon", "JP"),
        new("Coree du Sud", "KR"),
        new("Royaume-Uni", "GB"),
        new("Canada", "CA"),
        new("Espagne", "ES"),
        new("Allemagne", "DE")
    ];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowEmptyState))]
    [NotifyCanExecuteChangedFor(nameof(SaveMediaCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteMediaCommand))]
    private bool isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowEmptyState))]
    private bool hasItems;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowEmptyState))]
    private bool hasError;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private bool isCreatePanelOpen;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveMediaCommand))]
    private bool isEditMode;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveMediaCommand))]
    private MediaStatusOption? selectedMediaStatus;

    [ObservableProperty]
    private string personalRatingText = string.Empty;

    [ObservableProperty]
    private bool isFavorite;

    [ObservableProperty]
    private string currentSeasonText = string.Empty;

    [ObservableProperty]
    private string currentEpisodeText = string.Empty;

    [ObservableProperty]
    private bool editingSupportsEpisodeProgress;

    [ObservableProperty]
    private string? newPersonalNotes;

    [ObservableProperty]
    private bool hasFormError;

    [ObservableProperty]
    private string? formErrorMessage;

    [ObservableProperty]
    private string librarySearchQuery = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    private string searchQuery = string.Empty;

    [ObservableProperty]
    private string discoveryYear = string.Empty;

    [ObservableProperty]
    private string actorQuery = string.Empty;

    [ObservableProperty]
    private string directorQuery = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    [NotifyCanExecuteChangedFor(nameof(AddSearchResultCommand))]
    private bool isSearching;

    [ObservableProperty]
    private bool hasSearchResults;

    [ObservableProperty]
    private bool hasSearchError;

    [ObservableProperty]
    private string? searchErrorMessage;

    [ObservableProperty]
    private bool isLibraryPage = true;

    [ObservableProperty]
    private bool isDiscoverPage;

    [ObservableProperty]
    private bool isDiscoveryBrowseSelected = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowEmptyDiscoveryHistory))]
    private bool isDiscoveryHistorySelected;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowEmptyDiscoveryHistory))]
    private bool hasDiscoveryHistory;

    [ObservableProperty]
    private bool isDetailPage;

    [ObservableProperty]
    private bool isLibraryDetailPage;

    [ObservableProperty]
    private bool isSettingsPage;

    [ObservableProperty]
    private bool isBankPage;

    [ObservableProperty]
    private bool isAdminPage;

    [ObservableProperty]
    private string startingBalanceText = string.Empty;

    [ObservableProperty]
    private string bankEntryLabel = string.Empty;

    [ObservableProperty]
    private string bankEntryAmountText = string.Empty;

    [ObservableProperty]
    private string currentBalanceLabel = "0,00 €";

    [ObservableProperty]
    private string? bankErrorMessage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowNoDislikedPreferences))]
    private bool hasDislikedPreferences;

    [ObservableProperty]
    private string dislikedPreferencesSearchQuery = string.Empty;

    [ObservableProperty]
    private MediaSearchResultViewModel? selectedSearchResult;

    [ObservableProperty]
    private MediaListItemViewModel? selectedLibraryItem;

    [ObservableProperty]
    private MediaDetailsViewModel? selectedMediaDetails;

    [ObservableProperty]
    private SeasonSummaryViewModel? selectedSeason;

    [ObservableProperty]
    private SeasonDetailsViewModel? selectedSeasonDetails;

    [ObservableProperty]
    private bool isLoadingDetails;

    [ObservableProperty]
    private bool hasDetailsError;

    [ObservableProperty]
    private string? detailsErrorMessage;

    [ObservableProperty]
    private string? detailsStatusMessage;

    public bool ShowEmptyState => !IsLoading && !HasError && !HasItems;
    public bool ShowNoDislikedPreferences => !HasDislikedPreferences;
    public bool ShowNoFilteredDislikedPreferences =>
        HasDislikedPreferences && FilteredDislikedPreferences.Count == 0;
    public bool HasFilteredItems => FilteredItems.Count > 0;
    public bool ShowNoLibraryResults => HasItems && !HasFilteredItems;
    public bool ShowEmptyDiscoveryHistory => IsDiscoveryHistorySelected && !HasDiscoveryHistory;
    private bool CanSaveMedia =>
        !IsLoading &&
        IsEditMode &&
        editingMediaId is not null &&
        SelectedMediaStatus is not null;

    private bool CanDeleteMedia(MediaListItemViewModel? item) => !IsLoading && item is not null;
    private bool CanSearch => !IsSearching;
    private bool CanAddSearchResult(MediaSearchResultViewModel? item) => !IsSearching && item is not null;

    partial void OnLibrarySearchQueryChanged(string value) => RefreshLibraryFilter();

    partial void OnDislikedPreferencesSearchQueryChanged(string value) =>
        RefreshDislikedPreferencesFilter();

    partial void OnSearchQueryChanged(string value)
    {
        searchDebounceCancellation?.Cancel();
        var version = ++searchVersion;
        IsSearching = false;
        SearchResults.Clear();
        HasSearchResults = false;
        HasSearchError = false;
        SearchErrorMessage = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        searchDebounceCancellation = new CancellationTokenSource();
        _ = SearchAfterDelayAsync(value.Trim(), version, searchDebounceCancellation.Token);
    }

    [RelayCommand]
    private void ShowLibrary()
    {
        CloseForm();
        SetPage(library: true);
    }

    [RelayCommand]
    private async Task ShowBankAsync()
    {
        CloseForm();
        SetPage(bank: true);
        await LoadBankAsync();
    }

    [RelayCommand]
    private void ShowAdmin()
    {
        CloseForm();
        SetPage(admin: true);
    }

    [RelayCommand]
    private async Task SaveStartingBalanceAsync()
    {
        if (!TryParseMoney(StartingBalanceText, out var amount)) { BankErrorMessage = "Le solde doit etre un nombre valide."; return; }
        try { ApplyBankSummary(await bankApiClient.SetStartingBalanceAsync(amount)); BankErrorMessage = null; }
        catch (HttpRequestException) { BankErrorMessage = "Impossible d'enregistrer le solde."; }
    }

    [RelayCommand]
    private Task AddIncomeAsync() => AddBankEntryAsync(1);

    [RelayCommand]
    private Task AddExpenseAsync() => AddBankEntryAsync(-1);

    private async Task AddBankEntryAsync(int sign)
    {
        if (string.IsNullOrWhiteSpace(BankEntryLabel) || !TryParseMoney(BankEntryAmountText, out var amount) || amount <= 0)
        { BankErrorMessage = "Indique un intitule et un montant positif."; return; }
        try
        {
            await bankApiClient.AddEntryAsync(BankEntryLabel.Trim(), Math.Abs(amount) * sign);
            BankEntryLabel = BankEntryAmountText = string.Empty;
            BankErrorMessage = null;
            await LoadBankAsync();
        }
        catch (HttpRequestException) { BankErrorMessage = "Impossible d'ajouter cette operation."; }
    }

    [RelayCommand]
    private async Task DeleteBankEntryAsync(BankEntryViewModel entry)
    {
        try { await bankApiClient.DeleteEntryAsync(entry.Id); await LoadBankAsync(); }
        catch (HttpRequestException) { BankErrorMessage = "Impossible de supprimer cette operation."; }
    }

    [RelayCommand]
    private async Task ResetBankMonthAsync()
    {
        if (!userDialogService.ConfirmBankMonthReset()) return;
        try { await bankApiClient.ResetCurrentMonthAsync(); await LoadBankAsync(); }
        catch (HttpRequestException) { BankErrorMessage = "Impossible de reinitialiser le mois."; }
    }

    private async Task LoadBankAsync()
    {
        try { ApplyBankSummary(await bankApiClient.GetAsync()); BankErrorMessage = null; }
        catch (HttpRequestException) { BankErrorMessage = "Impossible de charger la comptabilite."; }
    }

    private void ApplyBankSummary(Nookly.Contracts.Banking.BankSummaryResponse summary)
    {
        StartingBalanceText = summary.StartingBalance.ToString("0.##", CultureInfo.CurrentCulture);
        CurrentBalanceLabel = $"{summary.CurrentBalance:N2} €";
        BankEntries.Clear();
        foreach (var entry in summary.Entries) BankEntries.Add(new BankEntryViewModel(entry));
        BankHistory.Clear();
        foreach (var month in summary.History ?? []) BankHistory.Add(new BankMonthViewModel(month));
    }

    private static bool TryParseMoney(string text, out decimal amount) =>
        decimal.TryParse(text.Trim().Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out amount);

    [RelayCommand]
    private async Task OpenDiscoverAsync()
    {
        CloseForm();
        SetPage(discover: true);
        IsDiscoveryBrowseSelected = true;
        IsDiscoveryHistorySelected = false;
        await LoadRecommendationsAsync();
    }

    [RelayCommand]
    private void ShowDiscoveryBrowse()
    {
        IsDiscoveryBrowseSelected = true;
        IsDiscoveryHistorySelected = false;
    }

    [RelayCommand]
    private async Task ShowDiscoveryHistoryAsync()
    {
        IsDiscoveryBrowseSelected = false;
        IsDiscoveryHistorySelected = true;
        try
        {
            var items = await mediaApiClient.GetDiscoveryHistoryAsync();
            DiscoveryHistory.Clear();
            foreach (var item in items) DiscoveryHistory.Add(new DiscoveryHistoryViewModel(item));
            HasDiscoveryHistory = DiscoveryHistory.Count > 0;
        }
        catch (HttpRequestException)
        {
            HasSearchError = true;
            SearchErrorMessage = "Impossible de charger l'historique.";
        }
    }

    [RelayCommand]
    private async Task ShowHistoryItemAsync(DiscoveryHistoryViewModel item)
    {
        var result = new MediaSearchResultViewModel(item.ToSearchResult());
        SelectedSearchResult = result;
        SetPage(detail: true);
        await LoadMediaDetailsAsync(result.Result.ExternalId, result.Result.Type);
    }

    [RelayCommand]
    private Task RefreshRecommendationsAsync() => LoadRecommendationsAsync();

    [RelayCommand]
    private async Task ToggleDiscoveryTypeAsync(DiscoveryTypeOption option)
    {
        option.IsSelected = !option.IsSelected;
        await RefreshDiscoveryFiltersAsync();
    }

    [RelayCommand]
    private async Task ToggleDiscoveryGenreAsync(GenreOption option)
    {
        option.IsSelected = !option.IsSelected;
        await RefreshDiscoveryFiltersAsync();
    }

    [RelayCommand]
    private async Task ToggleDiscoveryCountryAsync(CountryOption option)
    {
        option.IsSelected = !option.IsSelected;
        await RefreshDiscoveryFiltersAsync();
    }

    private async Task RefreshDiscoveryFiltersAsync()
    {
        searchDebounceCancellation?.Cancel();
        var version = ++searchVersion;
        await ExecuteSearchAsync(SearchQuery.Trim(), version, CancellationToken.None);
    }

    private async Task LoadRecommendationsAsync()
    {
        searchDebounceCancellation?.Cancel();
        SearchQuery = string.Empty;
        ActorQuery = string.Empty;
        DirectorQuery = string.Empty;
        DiscoveryYear = string.Empty;
        foreach (var option in DiscoveryMediaTypes) option.IsSelected = false;
        foreach (var option in DiscoveryGenres) option.IsSelected = false;
        foreach (var option in DiscoveryCountries) option.IsSelected = false;
        var version = ++searchVersion;
        await ExecuteSearchAsync(string.Empty, version, CancellationToken.None);
    }

    [RelayCommand]
    private async Task ShowSearchResultAsync(MediaSearchResultViewModel item)
    {
        SelectedSearchResult = item;
        SetPage(detail: true);
        await LoadMediaDetailsAsync(item.Result.ExternalId, item.Result.Type);
    }

    [RelayCommand]
    private async Task BackToDiscoverAsync()
    {
        SetPage(discover: true);
        if (IsDiscoveryHistorySelected) await ShowDiscoveryHistoryAsync();
    }

    [RelayCommand]
    private async Task ShowLibraryItemAsync(MediaListItemViewModel item)
    {
        SelectedLibraryItem = item;
        SetPage(libraryDetail: true);
        if (item.ExternalSource == "tmdb" && item.ExternalId is not null)
        {
            await LoadMediaDetailsAsync(item.ExternalId, item.Type);
        }
        else
        {
            SelectedMediaDetails = null;
        }
    }

    [RelayCommand]
    private void BackToLibrary() => SetPage(library: true);

    partial void OnSelectedSeasonChanged(SeasonSummaryViewModel? value)
    {
        if (value is not null && SelectedMediaDetails is not null)
        {
            _ = LoadSeasonDetailsAsync(SelectedMediaDetails.ExternalId, value.Number);
        }
    }

    [RelayCommand]
    private void OpenTrailer()
    {
        if (SelectedMediaDetails?.TrailerUrl is not { } trailerUrl) return;
        Process.Start(new ProcessStartInfo(trailerUrl) { UseShellExecute = true });
    }

    private async Task LoadMediaDetailsAsync(string externalId, MediaType type)
    {
        detailsCancellation?.Cancel();
        detailsCancellation = new CancellationTokenSource();
        var cancellationToken = detailsCancellation.Token;
        IsLoadingDetails = true;
        HasDetailsError = false;
        DetailsErrorMessage = null;
        SelectedMediaDetails = null;
        SelectedSeason = null;
        SelectedSeasonDetails = null;
        try
        {
            var details = await mediaApiClient.GetMediaDetailsAsync(externalId, type, cancellationToken);
            SelectedMediaDetails = new MediaDetailsViewModel(details);
            SelectedSeason = SelectedMediaDetails.Seasons.FirstOrDefault();
        }
        catch (OperationCanceledException)
        {
        }
        catch (HttpRequestException)
        {
            HasDetailsError = true;
            DetailsErrorMessage = "Impossible de charger les informations detaillees.";
        }
        finally
        {
            if (!cancellationToken.IsCancellationRequested) IsLoadingDetails = false;
        }
    }

    private async Task LoadSeasonDetailsAsync(string externalId, int seasonNumber)
    {
        try
        {
            IsLoadingDetails = true;
            DetailsStatusMessage = null;
            SelectedSeasonDetails = null;
            var season = await mediaApiClient.GetSeasonDetailsAsync(externalId, seasonNumber);
            var progress = await mediaApiClient.GetSeasonProgressAsync(
                externalId, seasonNumber, season.Episodes.Count);
            if (SelectedSeason?.Number == seasonNumber)
            {
                SelectedSeasonDetails = new SeasonDetailsViewModel(season, progress);
            }
        }
        catch (HttpRequestException)
        {
            HasDetailsError = true;
            DetailsErrorMessage = "Impossible de charger cette saison.";
        }
        finally
        {
            IsLoadingDetails = false;
        }
    }

    [RelayCommand]
    private async Task SaveSeasonProgressAsync()
    {
        if (SelectedMediaDetails is null || SelectedSeasonDetails is null) return;
        try
        {
            await mediaApiClient.UpdateSeasonProgressAsync(
                SelectedMediaDetails.ExternalId,
                SelectedSeasonDetails.Number,
                new UpdateSeasonProgressRequest(SelectedSeasonDetails.PersonalNotes));
            DetailsStatusMessage = "Note de saison enregistree.";
            HasDetailsError = false;
        }
        catch (HttpRequestException)
        {
            HasDetailsError = true;
            DetailsErrorMessage = "Impossible d'enregistrer la note de cette saison.";
        }
    }

    [RelayCommand]
    private async Task SaveEpisodeProgressAsync(EpisodeViewModel episode)
    {
        if (SelectedMediaDetails is null || SelectedSeasonDetails is null) return;
        try
        {
            await mediaApiClient.UpdateEpisodeProgressAsync(
                SelectedMediaDetails.ExternalId,
                SelectedSeasonDetails.Number,
                episode.Number,
                new UpdateEpisodeProgressRequest(episode.IsWatched, episode.PersonalNotes));
            SelectedSeasonDetails.RefreshWatchedCount();
            DetailsStatusMessage = $"Progression de l'episode {episode.Number} enregistree.";
            HasDetailsError = false;
        }
        catch (HttpRequestException)
        {
            HasDetailsError = true;
            DetailsErrorMessage = $"Impossible d'enregistrer l'episode {episode.Number}.";
        }
    }

    [RelayCommand]
    private async Task ShowSettingsAsync()
    {
        CloseForm();
        SetPage(settings: true);
        await LoadDislikedPreferencesAsync();
    }

    [RelayCommand]
    private async Task RestoreDiscoveryPreferenceAsync(DiscoveryPreferenceViewModel item)
    {
        try
        {
            await mediaApiClient.RestoreDiscoveryPreferenceAsync(item.ExternalSource, item.ExternalId);
            DislikedPreferences.Remove(item);
            HasDislikedPreferences = DislikedPreferences.Count > 0;
            RefreshDislikedPreferencesFilter();
        }
        catch (HttpRequestException)
        {
            HasError = true;
            ErrorMessage = "Impossible de restaurer ce titre.";
        }
    }

    private async Task LoadDislikedPreferencesAsync()
    {
        try
        {
            var preferences = await mediaApiClient.GetDislikedPreferencesAsync();
            DislikedPreferences.Clear();
            foreach (var preference in preferences)
            {
                DislikedPreferences.Add(new DiscoveryPreferenceViewModel(preference));
            }

            HasDislikedPreferences = DislikedPreferences.Count > 0;
            RefreshDislikedPreferencesFilter();
        }
        catch (HttpRequestException)
        {
            HasError = true;
            ErrorMessage = "Impossible de charger les parametres.";
        }
    }

    [RelayCommand(CanExecute = nameof(CanSearch))]
    private async Task SearchAsync()
    {
        searchDebounceCancellation?.Cancel();
        var version = ++searchVersion;
        await ExecuteSearchAsync(SearchQuery.Trim(), version, CancellationToken.None);
    }

    private async Task SearchAfterDelayAsync(
        string query,
        int version,
        CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(350, cancellationToken);
            await ExecuteSearchAsync(query, version, cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task ExecuteSearchAsync(
        string query,
        int version,
        CancellationToken cancellationToken)
    {
        if (version != searchVersion)
        {
            return;
        }

        IsSearching = true;
        HasSearchError = false;
        SearchErrorMessage = null;

        try
        {
            int? year = int.TryParse(DiscoveryYear, out var parsedYear) ? parsedYear : null;
            var results = await mediaApiClient.SearchMediaAsync(
                query,
                DiscoveryMediaTypes.Where(item => item.IsSelected).Select(item => item.Value).ToArray(),
                DiscoveryGenres.Where(item => item.IsSelected).Select(item => item.Value).ToArray(),
                year,
                ActorQuery,
                DirectorQuery,
                DiscoveryCountries.Where(item => item.IsSelected).Select(item => item.Code).ToArray(),
                cancellationToken);
            if (version != searchVersion)
            {
                return;
            }

            SearchResults.Clear();
            foreach (var result in results.Where(result => !IsInLibrary(result)))
            {
                SearchResults.Add(new MediaSearchResultViewModel(result));
            }

            HasSearchResults = SearchResults.Count > 0;
            if (!HasSearchResults)
            {
                HasSearchError = true;
                SearchErrorMessage = results.Count > 0
                    ? "Tous les resultats trouves sont deja dans ta bibliotheque."
                    : "Aucun film, serie ou anime trouve.";
            }
        }
        catch (HttpRequestException) when (version == searchVersion)
        {
            HasSearchError = true;
            SearchErrorMessage = "La recherche est indisponible. Verifie la configuration TMDB de l'API.";
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested && version == searchVersion)
        {
            HasSearchError = true;
            SearchErrorMessage = "La recherche a pris trop de temps.";
        }
        finally
        {
            if (version == searchVersion)
            {
                IsSearching = false;
            }
        }
    }

    private bool IsInLibrary(MediaSearchResultResponse result) =>
        Items.Any(item =>
            item.Type == result.Type &&
            string.Equals(item.ExternalSource, result.ExternalSource, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(item.ExternalId, result.ExternalId, StringComparison.Ordinal));

    [RelayCommand(CanExecute = nameof(CanAddSearchResult))]
    private async Task AddSearchResultAsync(MediaSearchResultViewModel item)
    {
        IsSearching = true;
        HasSearchError = false;
        SearchErrorMessage = null;

        try
        {
            var result = item.Result;
            var request = new CreateMediaRequest(
                result.Title,
                result.Type,
                result.Description,
                result.ExternalSource,
                result.ExternalId,
                result.PosterUrl,
                result.CommunityRating,
                result.ReleaseDate);
            var saved = await mediaApiClient.CreateMediaAsync(request);
            Items.Insert(0, new MediaListItemViewModel(saved));
            SearchResults.Remove(item);
            HasSearchResults = SearchResults.Count > 0;
            HasItems = true;
            RefreshLibraryFilter();
            SetPage(library: true);
        }
        catch (HttpRequestException exception)
        {
            HasSearchError = true;
            SearchErrorMessage = exception.StatusCode == System.Net.HttpStatusCode.Conflict
                ? "Ce media est deja dans ta bibliotheque."
                : "Impossible d'ajouter ce media.";
        }
        finally
        {
            IsSearching = false;
        }
    }

    [RelayCommand]
    private Task LikeDiscoveryResultAsync(MediaSearchResultViewModel item) =>
        SaveDiscoveryPreferenceAsync(item, true);

    [RelayCommand]
    private Task DislikeDiscoveryResultAsync(MediaSearchResultViewModel item) =>
        SaveDiscoveryPreferenceAsync(item, false);

    private async Task SaveDiscoveryPreferenceAsync(
        MediaSearchResultViewModel item,
        bool isLiked)
    {
        try
        {
            await mediaApiClient.SetDiscoveryPreferenceAsync(new SetDiscoveryPreferenceRequest(
                item.Result.ExternalSource,
                item.Result.ExternalId,
                item.Result.Title,
                isLiked,
                item.Result.Type));
            SearchResults.Remove(item);
            HasSearchResults = SearchResults.Count > 0;
        }
        catch (HttpRequestException)
        {
            HasSearchError = true;
            SearchErrorMessage = "Impossible d'enregistrer cette preference.";
        }
    }

    [RelayCommand]
    private void CancelEdit() => CloseForm();

    [RelayCommand]
    private void EditMedia(MediaListItemViewModel item)
    {
        SetPage(library: true);
        editingMediaId = item.Id;
        IsEditMode = true;
        SelectedMediaStatus = MediaStatuses.Single(option => option.Value == item.Status);
        PersonalRatingText = item.PersonalRating?.ToString("0.##", CultureInfo.CurrentCulture) ?? string.Empty;
        NewPersonalNotes = item.PersonalNotes;
        IsFavorite = item.IsFavorite;
        CurrentSeasonText = item.CurrentSeason?.ToString(CultureInfo.CurrentCulture) ?? string.Empty;
        CurrentEpisodeText = item.CurrentEpisode?.ToString(CultureInfo.CurrentCulture) ?? string.Empty;
        EditingSupportsEpisodeProgress = item.SupportsEpisodeProgress;
        HasFormError = false;
        FormErrorMessage = null;
        IsCreatePanelOpen = true;
    }

    [RelayCommand(CanExecute = nameof(CanSaveMedia))]
    private async Task SaveMediaAsync()
    {
        if (SelectedMediaStatus is null)
        {
            return;
        }

        IsLoading = true;
        HasFormError = false;
        FormErrorMessage = null;

        try
        {
            if (editingMediaId is not Guid id)
            {
                return;
            }

            if (!TryParseRating(out var rating) ||
                !TryParseOptionalPositiveInt(CurrentSeasonText, out var season) ||
                !TryParseOptionalPositiveInt(CurrentEpisodeText, out var episode))
            {
                HasFormError = true;
                FormErrorMessage = "La note doit être comprise entre 0 et 10, et la progression doit contenir des nombres positifs.";
                return;
            }

            var request = new UpdateMediaRequest(
                SelectedMediaStatus.Value,
                rating,
                NewPersonalNotes,
                IsFavorite,
                season,
                episode);
            var saved = await mediaApiClient.UpdateMediaAsync(id, request);

            var existing = Items.First(item => item.Id == id);
            var index = Items.IndexOf(existing);
            Items[index] = new MediaListItemViewModel(saved);
            SelectedLibraryItem = Items[index];

            HasItems = Items.Count > 0;
            RefreshLibraryFilter();
            CloseForm();
        }
        catch (HttpRequestException)
        {
            HasFormError = true;
            FormErrorMessage = "Impossible d'enregistrer ce media. Verifie que l'API est demarree.";
        }
        catch (TaskCanceledException)
        {
            HasFormError = true;
            FormErrorMessage = "L'enregistrement a pris trop de temps. Reessaie dans un instant.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanDeleteMedia))]
    private async Task DeleteMediaAsync(MediaListItemViewModel item)
    {
        if (!userDialogService.ConfirmDelete(item.Title))
        {
            return;
        }

        IsLoading = true;
        try
        {
            await mediaApiClient.DeleteMediaAsync(item.Id);
            Items.Remove(item);
            HasItems = Items.Count > 0;
            RefreshLibraryFilter();

            if (editingMediaId == item.Id)
            {
                CloseForm();
            }
        }
        catch (HttpRequestException)
        {
            HasError = true;
            ErrorMessage = "Impossible de supprimer ce media.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsLoading)
        {
            return;
        }

        IsLoading = true;
        HasError = false;
        ErrorMessage = null;

        try
        {
            var mediaItems = await mediaApiClient.GetMediaAsync();

            Items.Clear();
            foreach (var item in mediaItems)
            {
                Items.Add(new MediaListItemViewModel(item));
            }

            HasItems = Items.Count > 0;
            RefreshLibraryFilter();
        }
        catch (HttpRequestException)
        {
            HasError = true;
            ErrorMessage = "Impossible de joindre l'API Nookly. Verifie qu'elle est demarree.";
        }
        catch (TaskCanceledException)
        {
            HasError = true;
            ErrorMessage = "La requete a pris trop de temps. Reessaie dans un instant.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ResetForm()
    {
        editingMediaId = null;
        IsEditMode = false;
        SelectedMediaStatus = MediaStatuses[0];
        PersonalRatingText = string.Empty;
        NewPersonalNotes = null;
        IsFavorite = false;
        CurrentSeasonText = string.Empty;
        CurrentEpisodeText = string.Empty;
        EditingSupportsEpisodeProgress = false;
        HasFormError = false;
        FormErrorMessage = null;
    }

    private void CloseForm()
    {
        ResetForm();
        IsCreatePanelOpen = false;
    }

    private void SetPage(
        bool library = false,
        bool discover = false,
        bool detail = false,
        bool libraryDetail = false,
        bool settings = false,
        bool bank = false,
        bool admin = false)
    {
        IsLibraryPage = library;
        IsDiscoverPage = discover;
        IsDetailPage = detail;
        IsLibraryDetailPage = libraryDetail;
        IsSettingsPage = settings;
        IsBankPage = bank;
        IsAdminPage = admin;
    }

    private void RefreshLibraryFilter()
    {
        var query = LibrarySearchQuery.Trim();
        var filtered = string.IsNullOrEmpty(query)
            ? Items
            : Items.Where(item =>
                item.Title.Contains(query, StringComparison.CurrentCultureIgnoreCase));

        FilteredItems.Clear();
        foreach (var item in filtered)
        {
            FilteredItems.Add(item);
        }

        OnPropertyChanged(nameof(HasFilteredItems));
        OnPropertyChanged(nameof(ShowNoLibraryResults));
    }

    private void RefreshDislikedPreferencesFilter()
    {
        var query = DislikedPreferencesSearchQuery.Trim();
        var filtered = string.IsNullOrEmpty(query)
            ? DislikedPreferences
            : DislikedPreferences.Where(item =>
                item.Title.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                item.TypeLabel.Contains(query, StringComparison.CurrentCultureIgnoreCase));

        FilteredDislikedPreferences.Clear();
        foreach (var item in filtered)
        {
            FilteredDislikedPreferences.Add(item);
        }

        OnPropertyChanged(nameof(ShowNoFilteredDislikedPreferences));
    }

    private bool TryParseRating(out decimal? rating)
    {
        rating = null;
        if (string.IsNullOrWhiteSpace(PersonalRatingText))
        {
            return true;
        }

        var normalized = PersonalRatingText.Trim().Replace(',', '.');
        if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) ||
            value is < 0 or > 10)
        {
            return false;
        }

        rating = value;
        return true;
    }

    private static bool TryParseOptionalPositiveInt(string text, out int? value)
    {
        value = null;
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        if (!int.TryParse(text.Trim(), out var parsed) || parsed < 0)
        {
            return false;
        }

        value = parsed;
        return true;
    }
}
