namespace Nookly.Desktop.Services;

public interface IUserDialogService
{
    bool ConfirmDelete(string title);
    bool ConfirmBankMonthReset();
}
