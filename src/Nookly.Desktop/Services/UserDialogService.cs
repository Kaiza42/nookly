using System.Windows;

namespace Nookly.Desktop.Services;

public sealed class UserDialogService : IUserDialogService
{
    public bool ConfirmDelete(string title)
    {
        var result = MessageBox.Show(
            $"Supprimer '{title}' de la bibliotheque ?",
            "Confirmer la suppression",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        return result == MessageBoxResult.Yes;
    }
}
