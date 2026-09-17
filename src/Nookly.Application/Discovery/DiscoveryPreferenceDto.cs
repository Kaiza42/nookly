using Nookly.Domain.Media;

namespace Nookly.Application.Discovery;

public sealed record DiscoveryPreferenceDto(
    string ExternalSource,
    string ExternalId,
    string Title,
    MediaType? MediaType,
    bool IsLiked);
