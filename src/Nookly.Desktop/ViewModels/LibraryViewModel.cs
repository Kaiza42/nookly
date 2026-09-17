using System.Collections.ObjectModel;
using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nookly.Contracts.Media;
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

    public IReadOnlyList<RatingOption> Ratings { get; } = CreateRatingOptions();

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
    private bool isEditMode;

    [ObservableProperty]
    private MediaStatusOption? selectedMediaStatus;

    [ObservableProperty]
    private RatingOption? selectedRating;

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
    private MediaSearchResultViewModel? selectedSearchResult;

    public bool ShowEmptyState => !IsLoading && !HasError && !HasItems;
    public bool HasFilteredItems => FilteredItems.Count > 0;
    public bool ShowNoLibraryResults => HasItems && !HasFilteredItems;
    private bool CanSaveMedia =>
        !IsLoading &&
        IsEditMode &&
        editingMediaId is not null &&
        SelectedMediaStatus is not null;

    private bool CanDeleteMedia(MediaListItemViewModel? item) => !IsLoading && item is not null;
    private bool CanSearch => !IsSearching && !string.IsNullOrWhiteSpace(SearchQuery);
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
    private void OpenDiscover()
    {
        CloseForm();
        SetPage(discover: true);
    }

    [RelayCommand]
    private void ShowSearchResult(MediaSearchResultViewModel item)
    {
        SelectedSearchResult = item;
        SetPage(detail: true);
    }

    [RelayCommand]
    private void BackToDiscover() => SetPage(discover: true);

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
            var results = await mediaApiClient.SearchMediaAsync(query, cancellationToken);
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
    private void CancelEdit() => CloseForm();

    [RelayCommand]
    private void EditMedia(MediaListItemViewModel item)
    {
        editingMediaId = item.Id;
        IsEditMode = true;
        SelectedMediaStatus = MediaStatuses.Single(option => option.Value == item.Status);
        SelectedRating = Ratings.Single(option => option.Value == item.PersonalRating);
        NewPersonalNotes = item.PersonalNotes;
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

            var request = new UpdateMediaRequest(
                SelectedMediaStatus.Value,
                SelectedRating?.Value,
                NewPersonalNotes);
            var saved = await mediaApiClient.UpdateMediaAsync(id, request);

            var existing = Items.First(item => item.Id == id);
            var index = Items.IndexOf(existing);
            Items[index] = new MediaListItemViewModel(saved);

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
        SelectedRating = Ratings[0];
        NewPersonalNotes = null;
        HasFormError = false;
        FormErrorMessage = null;
    }

    private void CloseForm()
    {
        ResetForm();
        IsCreatePanelOpen = false;
    }

    private void SetPage(bool library = false, bool discover = false, bool detail = false)
    {
        IsLibraryPage = library;
        IsDiscoverPage = discover;
        IsDetailPage = detail;
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

    private static IReadOnlyList<RatingOption> CreateRatingOptions()
    {
        var ratings = new List<RatingOption> { new("Sans note", null) };
        ratings.AddRange(Enumerable.Range(0, 21).Select(value =>
        {
            var rating = value / 2m;
            return new RatingOption($"{rating:0.#}/10", rating);
        }));
        return ratings;
    }
}
