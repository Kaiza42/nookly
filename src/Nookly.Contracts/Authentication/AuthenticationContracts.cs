namespace Nookly.Contracts.Authentication;

public sealed record RegisterRequest(string Email, string Password, string DisplayName, bool StaySignedIn);
public sealed record LoginRequest(string Email, string Password, bool StaySignedIn);
public sealed record AuthenticationResponse(string AccessToken, DateTimeOffset ExpiresAtUtc, MemberResponse Member);
public sealed record MemberResponse(Guid Id, string Email, string DisplayName);
