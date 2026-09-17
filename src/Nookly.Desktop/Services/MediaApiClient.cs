using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Nookly.Contracts.Media;

namespace Nookly.Desktop.Services;

public sealed class MediaApiClient(HttpClient httpClient) : IMediaApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    public async Task<IReadOnlyList<MediaItemResponse>> GetMediaAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync("api/media", cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<MediaItemResponse[]>(
                   JsonOptions,
                   cancellationToken)
               ?? [];
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
