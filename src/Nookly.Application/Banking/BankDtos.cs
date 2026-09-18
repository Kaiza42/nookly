namespace Nookly.Application.Banking;

public sealed record BankSummaryDto(decimal StartingBalance, decimal CurrentBalance, IReadOnlyList<BankEntryDto> Entries, IReadOnlyList<BankMonthDto> History);
public sealed record BankEntryDto(Guid Id, string Label, decimal Amount, DateTimeOffset CreatedAtUtc);
public sealed record BankMonthDto(int Year, int Month, decimal StartingBalance, decimal Income, decimal Expenses, decimal EndingBalance, IReadOnlyList<BankEntryDto> Entries);
