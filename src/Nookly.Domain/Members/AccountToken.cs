namespace Nookly.Domain.Members;

public sealed class AccountToken
{
    private AccountToken() { }
    private AccountToken(Guid memberId, string tokenHash, AccountTokenPurpose purpose, DateTimeOffset expiresAtUtc)
    {
        Id = Guid.NewGuid(); MemberId = memberId; TokenHash = tokenHash; Purpose = purpose;
        ExpiresAtUtc = expiresAtUtc; CreatedAtUtc = DateTimeOffset.UtcNow;
    }
    public Guid Id { get; private set; }
    public Guid MemberId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public AccountTokenPurpose Purpose { get; private set; }
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UsedAtUtc { get; private set; }
    public static AccountToken Create(Guid memberId, string hash, AccountTokenPurpose purpose, DateTimeOffset expires) =>
        new(memberId, hash, purpose, expires);
    public bool CanUse(DateTimeOffset now) => UsedAtUtc is null && ExpiresAtUtc > now;
    public void MarkUsed(DateTimeOffset now) => UsedAtUtc = now;
}

public enum AccountTokenPurpose { ConfirmEmail, ResetPassword }
