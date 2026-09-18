namespace Nookly.Domain.Members;

public sealed class MemberNotification
{
    private MemberNotification() { }
    private MemberNotification(Guid memberId, string title, string message)
    { Id = Guid.NewGuid(); MemberId = memberId; Title = title.Trim(); Message = message.Trim(); CreatedAtUtc = DateTimeOffset.UtcNow; }
    public Guid Id { get; private set; }
    public Guid MemberId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? ReadAtUtc { get; private set; }
    public static MemberNotification Create(Guid memberId, string title, string message)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Title and message are required.");
        return new MemberNotification(memberId, title, message);
    }
    public void MarkRead() => ReadAtUtc ??= DateTimeOffset.UtcNow;
}
