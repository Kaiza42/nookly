using Nookly.Domain.Banking;

namespace Nookly.Application.Abstractions;

public interface IBankRepository
{
    Task<BankAccount?> GetAccountAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<BankEntry>> ListEntriesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<BankAccount>> ListAccountsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<BankEntry>> ListAllEntriesAsync(CancellationToken cancellationToken);
    Task SaveAccountAsync(BankAccount account, CancellationToken cancellationToken);
    Task AddEntryAsync(BankEntry entry, CancellationToken cancellationToken);
    Task<bool> DeleteEntryAsync(Guid id, CancellationToken cancellationToken);
    Task ResetCurrentMonthAsync(BankAccount account, CancellationToken cancellationToken);
}
