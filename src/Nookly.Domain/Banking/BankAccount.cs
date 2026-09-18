namespace Nookly.Domain.Banking;

public sealed class BankAccount
{
    private BankAccount() { }
    private BankAccount(Guid memberId, int year, int month)
    { Id = Guid.NewGuid(); MemberId = memberId; Year = year; Month = month; UpdatedAtUtc = DateTimeOffset.UtcNow; }
    public Guid Id { get; private set; }
    public Guid MemberId { get; private set; }
    public int Year { get; private set; }
    public int Month { get; private set; }
    public decimal StartingBalance { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public static BankAccount Create(Guid memberId, int year, int month)
    {
        if (year is < 2000 or > 9999 || month is < 1 or > 12) throw new ArgumentOutOfRangeException(nameof(month));
        return new BankAccount(memberId, year, month);
    }
    public void SetStartingBalance(decimal amount)
    {
        if (amount is < -999999999 or > 999999999) throw new ArgumentOutOfRangeException(nameof(amount));
        StartingBalance = amount;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
