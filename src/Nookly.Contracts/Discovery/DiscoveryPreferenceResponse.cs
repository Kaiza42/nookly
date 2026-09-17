using Nookly.Contracts.Media;

namespace Nookly.Contracts.Discovery;

public sealed record DiscoveryPreferenceResponse(
    string ExternalSource,
    string ExternalId,
    string Title,
    MediaType? Type,
    bool IsLiked);
