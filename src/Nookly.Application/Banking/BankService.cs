using Nookly.Application.Abstractions;
using Nookly.Domain.Banking;

namespace Nookly.Application.Banking;

public sealed class BankService(IBankRepository repository, ICurrentMember currentMember)
{
    public async Task<BankSummaryDto> GetAsync(CancellationToken token = default)
    {
        var account = await repository.GetAccountAsync(token);
        var entries = await repository.ListEntriesAsync(token);
        var startingBalance = account?.StartingBalance ?? 0;
        return new BankSummaryDto(startingBalance, startingBalance + entries.Sum(item => item.Amount),
            entries.Select(ToResponse).ToArray());
    }
    public async Task<BankSummaryDto> SetStartingBalanceAsync(decimal amount, CancellationToken token = default)
    {
        var account = await repository.GetAccountAsync(token) ?? BankAccount.Create(currentMember.Id);
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
    private static BankEntryDto ToResponse(BankEntry entry) => new(entry.Id, entry.Label, entry.Amount, entry.CreatedAtUtc);
}
