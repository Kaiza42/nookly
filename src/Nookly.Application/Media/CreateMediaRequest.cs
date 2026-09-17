using Nookly.Domain.Media;

namespace Nookly.Application.Media;

public sealed record CreateMediaRequest(
    string Title,
    MediaType Type,
    string? Description);
