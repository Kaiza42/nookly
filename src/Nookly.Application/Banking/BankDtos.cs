namespace Nookly.Application.Banking;

public sealed record BankSummaryDto(decimal StartingBalance, decimal CurrentBalance, IReadOnlyList<BankEntryDto> Entries);
public sealed record BankEntryDto(Guid Id, string Label, decimal Amount, DateTimeOffset CreatedAtUtc);
