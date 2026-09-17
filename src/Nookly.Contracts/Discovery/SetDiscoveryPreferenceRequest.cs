namespace Nookly.Contracts.Discovery;

public sealed record SetDiscoveryPreferenceRequest(
    string ExternalSource,
    string ExternalId,
    bool IsLiked);
