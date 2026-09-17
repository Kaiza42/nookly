using System.Collections.ObjectModel;
using System.Net.Http;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nookly.Contracts.Media;
using Nookly.Contracts.Discovery;
using Nookly.Desktop.Services;

namespace Nookly.Desktop.ViewModels;

public partial class LibraryViewModel(
    IMediaApiClient mediaApiClient,
    IUserDialogService userDialogService) : ObservableObject
{
    private Guid? editingMediaId;
    private CancellationTokenSource? searchDebounceCancellation;
    private int searchVersion;

    public ObservableCollection<MediaListItemViewModel> Items { get; } = [];
    public ObservableCollection<MediaListItemViewModel> FilteredItems { get; } = [];
    public ObservableCollection<MediaSearchResultViewModel> SearchResults { get; } = [];

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
        new("Tous les types", null),
        new("Films", MediaType.Movie),
        new("Series", MediaType.TvSeries),
        new("Animes", MediaType.Anime)
    ];

    public IReadOnlyList<GenreOption> DiscoveryGenres { get; } =
    [
        new("Tous les genres", null),
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
    private DiscoveryTypeOption? selectedDiscoveryType;

    [ObservableProperty]
    private GenreOption? selectedDiscoveryGenre;

    [ObservableProperty]
    private string discoveryYear = string.Empty;

    [ObservableProperty]
    private string actorQuery = string.Empty;

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
    private bool isDetailPage;

    [ObservableProperty]
    private bool isLibraryDetailPage;

    [ObservableProperty]
    private MediaSearchResultViewModel? selectedSearchResult;

    [ObservableProperty]
    private MediaListItemViewModel? selectedLibraryItem;

    public bool ShowEmptyState => !IsLoading && !HasError && !HasItems;
    public bool HasFilteredItems => FilteredItems.Count > 0;
    public bool ShowNoLibraryResults => HasItems && !HasFilteredItems;
    private bool CanSaveMedia =>
        !IsLoading &&
        IsEditMode &&
        editingMediaId is not null &&
        SelectedMediaStatus is not null;

    private bool CanDeleteMedia(MediaListItemViewModel? item) => !IsLoading && item is not null;
    private bool CanSearch => !IsSearching;
    private bool CanAddSearchResult(MediaSearchResultViewModel? item) => !IsSearching && item is not null;

    partial void OnLibrarySearchQueryChanged(string value) => RefreshLibraryFilter();

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
    private async Task OpenDiscoverAsync()
    {
        CloseForm();
        SetPage(discover: true);
        await LoadRecommendationsAsync();
    }

    [RelayCommand]
    private Task RefreshRecommendationsAsync() => LoadRecommendationsAsync();

    private async Task LoadRecommendationsAsync()
    {
        searchDebounceCancellation?.Cancel();
        SearchQuery = string.Empty;
        ActorQuery = string.Empty;
        DiscoveryYear = string.Empty;
        SelectedDiscoveryType = DiscoveryMediaTypes[0];
        SelectedDiscoveryGenre = DiscoveryGenres[0];
        var version = ++searchVersion;
        await ExecuteSearchAsync(string.Empty, version, CancellationToken.None);
    }

    [RelayCommand]
    private void ShowSearchResult(MediaSearchResultViewModel item)
    {
        SelectedSearchResult = item;
        SetPage(detail: true);
    }

    [RelayCommand]
    private void BackToDiscover() => SetPage(discover: true);

    [RelayCommand]
    private void ShowLibraryItem(MediaListItemViewModel item)
    {
        SelectedLibraryItem = item;
        SetPage(libraryDetail: true);
    }

    [RelayCommand]
    private void BackToLibrary() => SetPage(library: true);

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
                SelectedDiscoveryType?.Value,
                SelectedDiscoveryGenre?.Value,
                year,
                ActorQuery,
                cancellationToken);
            if (version != searchVersion)
            {
                return;
            }

            SearchResults.Clear();
            foreach (var result in results)
            {
                SearchResults.Add(new MediaSearchResultViewModel(result));
            }

            HasSearchResults = SearchResults.Count > 0;
            if (!HasSearchResults)
            {
                HasSearchError = true;
                SearchErrorMessage = "Aucun film, serie ou anime trouve.";
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
        bool libraryDetail = false)
    {
        IsLibraryPage = library;
        IsDiscoverPage = discover;
        IsDetailPage = detail;
        IsLibraryDetailPage = libraryDetail;
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
