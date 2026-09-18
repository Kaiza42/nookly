namespace Nookly.Domain.Members;

public sealed class Member
{
    private Member() { }

    private Member(string email, string displayName, MemberRole role)
    {
        Id = Guid.NewGuid();
        Email = email.Trim().ToLowerInvariant();
        DisplayName = displayName.Trim();
        Role = role;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public MemberRole Role { get; private set; }
    public bool IsEmailConfirmed { get; private set; }
    public long UsageSeconds { get; private set; }
    public DateTimeOffset? LastActivityAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static Member Create(string email, string displayName, MemberRole role = MemberRole.Member)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("An email is required.");
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("A display name is required.");
        return new Member(email, displayName, role);
    }

    public void SetPasswordHash(string passwordHash) => PasswordHash =
        !string.IsNullOrWhiteSpace(passwordHash)
            ? passwordHash
            : throw new ArgumentException("A password hash is required.");

    public void ConfirmEmail() => IsEmailConfirmed = true;
    public void PromoteToAdmin() => Role = MemberRole.Admin;

    public void RecordActivity(DateTimeOffset now)
    {
        if (LastActivityAtUtc is not null)
        {
            var elapsed = (long)(now - LastActivityAtUtc.Value).TotalSeconds;
            if (elapsed is > 0 and <= 120) UsageSeconds += elapsed;
        }
        LastActivityAtUtc = now;
    }
}

public enum MemberRole { Member, Admin }
