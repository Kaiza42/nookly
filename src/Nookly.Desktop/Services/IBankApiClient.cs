using Nookly.Contracts.Banking;

namespace Nookly.Desktop.Services;

public interface IBankApiClient
{
    Task<BankSummaryResponse> GetAsync(CancellationToken token = default);
    Task<BankSummaryResponse> SetStartingBalanceAsync(decimal amount, CancellationToken token = default);
    Task<BankEntryResponse> AddEntryAsync(string label, decimal amount, CancellationToken token = default);
    Task DeleteEntryAsync(Guid id, CancellationToken token = default);
}
