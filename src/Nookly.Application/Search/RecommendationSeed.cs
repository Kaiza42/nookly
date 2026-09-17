using Nookly.Domain.Media;

namespace Nookly.Application.Search;

public sealed record RecommendationSeed(string ExternalId, MediaType Type);
