using System.Collections.ObjectModel;
using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nookly.Contracts.Media;
using Nookly.Desktop.Services;

namespace Nookly.Desktop.ViewModels;

public partial class LibraryViewModel(IMediaApiClient mediaApiClient) : ObservableObject
{
    public ObservableCollection<MediaListItemViewModel> Items { get; } = [];

    public IReadOnlyList<MediaTypeOption> MediaTypes { get; } =
    [
        new("Film", MediaType.Movie),
        new("Serie", MediaType.TvSeries),
        new("Anime", MediaType.Anime),
        new("Manga", MediaType.Manga)
    ];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowEmptyState))]
    [NotifyCanExecuteChangedFor(nameof(AddMediaCommand))]
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
    [NotifyCanExecuteChangedFor(nameof(AddMediaCommand))]
    private string newTitle = string.Empty;

    [ObservableProperty]
    private string? newDescription;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddMediaCommand))]
    private MediaTypeOption? selectedMediaType;

    [ObservableProperty]
    private bool hasFormError;

    [ObservableProperty]
    private string? formErrorMessage;

    public bool ShowEmptyState => !IsLoading && !HasError && !HasItems;

    private bool CanAddMedia =>
        !IsLoading &&
        !string.IsNullOrWhiteSpace(NewTitle) &&
        SelectedMediaType is not null;

    [RelayCommand]
    private void ToggleCreatePanel()
    {
        IsCreatePanelOpen = !IsCreatePanelOpen;
        HasFormError = false;
        FormErrorMessage = null;

        if (IsCreatePanelOpen && SelectedMediaType is null)
        {
            SelectedMediaType = MediaTypes[0];
        }
    }

    [RelayCommand(CanExecute = nameof(CanAddMedia))]
    private async Task AddMediaAsync()
    {
        if (SelectedMediaType is null)
        {
            return;
        }

        IsLoading = true;
        HasFormError = false;
        FormErrorMessage = null;

        try
        {
            var request = new CreateMediaRequest(
                NewTitle,
                SelectedMediaType.Value,
                NewDescription);
            var created = await mediaApiClient.CreateMediaAsync(request);

            Items.Insert(0, new MediaListItemViewModel(created));
            HasItems = true;
            NewTitle = string.Empty;
            NewDescription = null;
            IsCreatePanelOpen = false;
        }
        catch (HttpRequestException)
        {
            HasFormError = true;
            FormErrorMessage = "Impossible d'ajouter ce media. Verifie que l'API est demarree.";
        }
        catch (TaskCanceledException)
        {
            HasFormError = true;
            FormErrorMessage = "L'ajout a pris trop de temps. Reessaie dans un instant.";
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
}
