using Nookly.Application.Abstractions;
using Nookly.Domain.Banking;

namespace Nookly.Application.Banking;

public sealed class BankService(IBankRepository repository, ICurrentMember currentMember)
{
    public async Task<BankSummaryDto> GetAsync(CancellationToken token = default)
    {
        var account = await repository.GetAccountAsync(token);
        var entries = await repository.ListEntriesAsync(token);
        var accounts = await repository.ListAccountsAsync(token);
        var allEntries = await repository.ListAllEntriesAsync(token);
        var startingBalance = account?.StartingBalance ?? 0;
        var history = accounts.Select(monthAccount =>
        {
            var monthEntries = allEntries.Where(entry => entry.CreatedAtUtc.Year == monthAccount.Year && entry.CreatedAtUtc.Month == monthAccount.Month).ToArray();
            var income = monthEntries.Where(entry => entry.Amount > 0).Sum(entry => entry.Amount);
            var expenses = Math.Abs(monthEntries.Where(entry => entry.Amount < 0).Sum(entry => entry.Amount));
            return new BankMonthDto(monthAccount.Year, monthAccount.Month, monthAccount.StartingBalance, income, expenses,
                monthAccount.StartingBalance + income - expenses, monthEntries.Select(ToResponse).ToArray());
        }).OrderByDescending(item => item.Year).ThenByDescending(item => item.Month).ToArray();
        return new BankSummaryDto(startingBalance, startingBalance + entries.Sum(item => item.Amount),
            entries.Select(ToResponse).ToArray(), history);
    }
    public async Task<BankSummaryDto> SetStartingBalanceAsync(decimal amount, CancellationToken token = default)
    {
        var now = DateTimeOffset.UtcNow;
        var account = await repository.GetAccountAsync(token) ?? BankAccount.Create(currentMember.Id, now.Year, now.Month);
        account.SetStartingBalance(amount);
        await repository.SaveAccountAsync(account, token);
        return await GetAsync(token);
    }
    public async Task<BankEntryDto> AddEntryAsync(string label, decimal amount, CancellationToken token = default)
    {
        var entry = BankEntry.Create(currentMember.Id, label, amount);
        await repository.AddEntryAsync(entry, token);
        return ToResponse(entry);
    }
    public Task<bool> DeleteEntryAsync(Guid id, CancellationToken token = default) => repository.DeleteEntryAsync(id, token);
    public async Task ResetCurrentMonthAsync(CancellationToken token = default)
    {
        var now = DateTimeOffset.UtcNow;
        var account = await repository.GetAccountAsync(token) ?? BankAccount.Create(currentMember.Id, now.Year, now.Month);
        account.SetStartingBalance(0);
        await repository.ResetCurrentMonthAsync(account, token);
    }
    private static BankEntryDto ToResponse(BankEntry entry) => new(entry.Id, entry.Label, entry.Amount, entry.CreatedAtUtc);
}
