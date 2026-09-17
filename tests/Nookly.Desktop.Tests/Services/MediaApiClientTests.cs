using System.Net;
using System.Net.Http;
using System.Text;
using Nookly.Contracts.Media;
using Nookly.Desktop.Services;

namespace Nookly.Desktop.Tests.Services;

public sealed class MediaApiClientTests
{
    [Fact]
    public async Task GetMediaAsync_DeserializesApiResponse()
    {
        const string json = """
            [
              {
                "id": "4ddf7779-f0de-4f40-87c2-bdc7e5e9901a",
                "title": "Dune",
                "description": "Science fiction",
                "type": "Movie",
                "status": "Planned",
                "createdAtUtc": "2026-09-17T06:00:00+00:00"
              }
            ]
            """;

        using var httpClient = new HttpClient(new StubHttpMessageHandler(json))
        {
            BaseAddress = new Uri("http://localhost/")
        };
        var client = new MediaApiClient(httpClient);

        var result = await client.GetMediaAsync();

        var item = Assert.Single(result);
        Assert.Equal("Dune", item.Title);
        Assert.Equal(MediaType.Movie, item.Type);
        Assert.Equal(MediaStatus.Planned, item.Status);
    }

    private sealed class StubHttpMessageHandler(string content) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };

            return Task.FromResult(response);
        }
    }
}
