using System.Windows;

namespace Nookly.Desktop.Services;

public sealed class UserDialogService : IUserDialogService
{
    public bool ConfirmDelete(string title)
    {
        var result = System.Windows.MessageBox.Show(
            $"Supprimer '{title}' de la bibliotheque ?",
            "Confirmer la suppression",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        return result == MessageBoxResult.Yes;
    }

    public bool ConfirmBankMonthReset() => System.Windows.MessageBox.Show(
        "Reinitialiser le mois courant ? Son solde et toutes ses operations seront supprimes.",
        "Reinitialiser la banque",
        MessageBoxButton.YesNo,
        MessageBoxImage.Warning) == MessageBoxResult.Yes;
}
