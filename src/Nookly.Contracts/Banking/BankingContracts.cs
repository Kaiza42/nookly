namespace Nookly.Contracts.Banking;

public sealed record BankSummaryResponse(decimal StartingBalance, decimal CurrentBalance, IReadOnlyList<BankEntryResponse> Entries, IReadOnlyList<BankMonthResponse>? History = null);
public sealed record BankEntryResponse(Guid Id, string Label, decimal Amount, DateTimeOffset CreatedAtUtc);
public sealed record BankMonthResponse(int Year, int Month, decimal StartingBalance, decimal Income, decimal Expenses, decimal EndingBalance, IReadOnlyList<BankEntryResponse> Entries);
public sealed record SetStartingBalanceRequest(decimal Amount);
public sealed record CreateBankEntryRequest(string Label, decimal Amount);
