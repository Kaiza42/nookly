namespace Nookly.Contracts.Administration;

public sealed record AdminMemberResponse(Guid Id, string Email, string DisplayName, bool IsEmailConfirmed,
    long UsageSeconds, DateTimeOffset? LastActivityAtUtc, DateTimeOffset CreatedAtUtc);
public sealed record SendNotificationRequest(Guid MemberId, string Title, string Message);
public sealed record NotificationResponse(Guid Id, string Title, string Message, DateTimeOffset CreatedAtUtc);
