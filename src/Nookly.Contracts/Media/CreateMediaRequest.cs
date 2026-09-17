namespace Nookly.Contracts.Media;

public sealed record CreateMediaRequest(
    string Title,
    MediaType Type,
    string? Description);
