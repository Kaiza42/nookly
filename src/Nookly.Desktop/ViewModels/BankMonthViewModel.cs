using System.Globalization;
using Nookly.Contracts.Banking;

namespace Nookly.Desktop.ViewModels;

public sealed class BankMonthViewModel(BankMonthResponse month)
{
    public string MonthLabel => new DateTime(month.Year, month.Month, 1).ToString("MMMM yyyy", CultureInfo.GetCultureInfo("fr-FR"));
    public string SummaryLabel => $"Entrees +{month.Income:N2} €  |  Depenses -{month.Expenses:N2} €  |  Solde {month.EndingBalance:N2} €";
    public IReadOnlyList<BankEntryViewModel> Entries => month.Entries.Select(entry => new BankEntryViewModel(entry)).ToArray();
}
