namespace Nookly.Contracts.Authentication;

public sealed record RegisterRequest(string Email, string Password, string DisplayName, bool StaySignedIn);
public sealed record LoginRequest(string Email, string Password, bool StaySignedIn);
public sealed record AuthenticationResponse(string AccessToken, DateTimeOffset ExpiresAtUtc, MemberResponse Member);
public sealed record MemberResponse(Guid Id, string Email, string DisplayName, string Role = "Member", bool IsEmailConfirmed = true);
public sealed record MessageResponse(string Message);
public sealed record ForgotPasswordRequest(string Email);
public sealed record ResetPasswordRequest(string Token, string NewPassword);
