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

    public ObservableCollection<MediaListItemViewModel> Items { get; } = [];
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
    [NotifyCanExecuteChangedFor(nameof(SaveMediaCommand))]
    private string newTitle = string.Empty;

    [ObservableProperty]
    private string? newDescription;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveMediaCommand))]
    private MediaTypeOption? selectedMediaType;

    [ObservableProperty]
    private MediaStatusOption? selectedMediaStatus;

    [ObservableProperty]
    private RatingOption? selectedRating;

    [ObservableProperty]
    private bool hasFormError;

    [ObservableProperty]
    private string? formErrorMessage;

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

    public bool ShowEmptyState => !IsLoading && !HasError && !HasItems;
    private bool CanSaveMedia =>
        !IsLoading &&
        IsEditMode &&
        editingMediaId is not null &&
        !string.IsNullOrWhiteSpace(NewTitle) &&
        SelectedMediaType is not null &&
        SelectedMediaStatus is not null;

    private bool CanDeleteMedia(MediaListItemViewModel? item) => !IsLoading && item is not null;
    private bool CanSearch => !IsSearching && !string.IsNullOrWhiteSpace(SearchQuery);
    private bool CanAddSearchResult(MediaSearchResultViewModel? item) => !IsSearching && item is not null;

    [RelayCommand(CanExecute = nameof(CanSearch))]
    private async Task SearchAsync()
    {
        IsSearching = true;
        HasSearchError = false;
        SearchErrorMessage = null;

        try
        {
            var results = await mediaApiClient.SearchMediaAsync(SearchQuery.Trim());
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
        catch (HttpRequestException)
        {
            HasSearchError = true;
            SearchErrorMessage = "La recherche est indisponible. Verifie la configuration TMDB de l'API.";
        }
        catch (TaskCanceledException)
        {
            HasSearchError = true;
            SearchErrorMessage = "La recherche a pris trop de temps.";
        }
        finally
        {
            IsSearching = false;
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
            SearchResults.Remove(item);
            HasSearchResults = SearchResults.Count > 0;
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
        NewTitle = item.Title;
        NewDescription = item.Description;
        SelectedMediaType = MediaTypes.Single(option => option.Value == item.Type);
        SelectedMediaStatus = MediaStatuses.Single(option => option.Value == item.Status);
        SelectedRating = Ratings.Single(option => option.Value == item.PersonalRating);
        HasFormError = false;
        FormErrorMessage = null;
        IsCreatePanelOpen = true;
    }

    [RelayCommand(CanExecute = nameof(CanSaveMedia))]
    private async Task SaveMediaAsync()
    {
        if (SelectedMediaType is null || SelectedMediaStatus is null)
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
                NewTitle,
                SelectedMediaType.Value,
                NewDescription,
                SelectedMediaStatus.Value,
                SelectedRating?.Value);
            var saved = await mediaApiClient.UpdateMediaAsync(id, request);

            var existing = Items.First(item => item.Id == id);
            var index = Items.IndexOf(existing);
            Items[index] = new MediaListItemViewModel(saved);

            HasItems = Items.Count > 0;
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
        NewTitle = string.Empty;
        NewDescription = null;
        SelectedMediaType = MediaTypes[0];
        SelectedMediaStatus = MediaStatuses[0];
        SelectedRating = Ratings[0];
        HasFormError = false;
        FormErrorMessage = null;
    }

    private void CloseForm()
    {
        ResetForm();
        IsCreatePanelOpen = false;
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
