namespace Nookly.Domain.Banking;

public sealed class BankEntry
{
    private BankEntry() { }
    private BankEntry(Guid memberId, string label, decimal amount)
    {
        Id = Guid.NewGuid(); MemberId = memberId; Label = label; Amount = amount; CreatedAtUtc = DateTimeOffset.UtcNow;
    }
    public Guid Id { get; private set; }
    public Guid MemberId { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public static BankEntry Create(Guid memberId, string label, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("A label is required.", nameof(label));
        if (amount == 0 || amount is < -999999999 or > 999999999) throw new ArgumentOutOfRangeException(nameof(amount));
        return new BankEntry(memberId, label.Trim(), amount);
    }
}
