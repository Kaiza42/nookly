namespace Nookly.Domain.Banking;

public sealed class BankAccount
{
    private BankAccount() { }
    private BankAccount(Guid memberId) { Id = Guid.NewGuid(); MemberId = memberId; UpdatedAtUtc = DateTimeOffset.UtcNow; }
    public Guid Id { get; private set; }
    public Guid MemberId { get; private set; }
    public decimal StartingBalance { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public static BankAccount Create(Guid memberId) => new(memberId);
    public void SetStartingBalance(decimal amount)
    {
        if (amount is < -999999999 or > 999999999) throw new ArgumentOutOfRangeException(nameof(amount));
        StartingBalance = amount;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
