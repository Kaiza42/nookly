namespace Nookly.Domain.Members;

public sealed class Member
{
    private Member() { }

    private Member(string email, string displayName)
    {
        Id = Guid.NewGuid();
        Email = email.Trim().ToLowerInvariant();
        DisplayName = displayName.Trim();
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static Member Create(string email, string displayName)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("An email is required.");
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("A display name is required.");
        return new Member(email, displayName);
    }

    public void SetPasswordHash(string passwordHash) => PasswordHash =
        !string.IsNullOrWhiteSpace(passwordHash)
            ? passwordHash
            : throw new ArgumentException("A password hash is required.");
}
