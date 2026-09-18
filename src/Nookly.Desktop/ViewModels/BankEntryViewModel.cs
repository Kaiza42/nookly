using Nookly.Contracts.Banking;

namespace Nookly.Desktop.ViewModels;

public sealed class BankEntryViewModel(BankEntryResponse entry)
{
    public Guid Id => entry.Id;
    public string Label => entry.Label;
    public decimal Amount => entry.Amount;
    public bool IsIncome => Amount > 0;
    public bool IsExpense => Amount < 0;
    public string AmountLabel => $"{(Amount > 0 ? "+" : "-")} {Math.Abs(Amount):N2} €";
    public string DateLabel => entry.CreatedAtUtc.LocalDateTime.ToString("dd/MM/yyyy HH:mm");
}
