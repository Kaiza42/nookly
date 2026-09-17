namespace Nookly.Contracts.Discovery;

using Nookly.Contracts.Media;

public sealed record SetDiscoveryPreferenceRequest(
    string ExternalSource,
    string ExternalId,
    string Title,
    bool IsLiked,
    MediaType Type);
