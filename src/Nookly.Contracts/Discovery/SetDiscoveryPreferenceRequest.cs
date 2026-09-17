namespace Nookly.Contracts.Discovery;

using Nookly.Contracts.Media;

public sealed record SetDiscoveryPreferenceRequest(
    string ExternalSource,
    string ExternalId,
    bool IsLiked,
    MediaType Type);
