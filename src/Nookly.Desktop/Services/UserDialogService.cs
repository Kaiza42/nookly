using System.Windows;

namespace Nookly.Desktop.Services;

public sealed class UserDialogService : IUserDialogService
{
    public bool ConfirmDelete(string title)
    {
        var dialog = new ConfirmationWindow(title)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };
        return dialog.ShowDialog() == true;
    }

    public bool ConfirmBankMonthReset() => System.Windows.MessageBox.Show(
        "Reinitialiser le mois courant ? Son solde et toutes ses operations seront supprimes.",
        "Reinitialiser la banque",
        MessageBoxButton.YesNo,
        MessageBoxImage.Warning) == MessageBoxResult.Yes;
}
