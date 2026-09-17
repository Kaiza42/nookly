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

    [Fact]
    public async Task CreateMediaAsync_SendsRequestAndDeserializesCreatedMedia()
    {
        const string json = """
            {
              "id": "5deaa2bf-e83a-4c00-a6db-275d4d06a49d",
              "title": "Berserk",
              "description": "Manga",
              "type": "Manga",
              "status": "Planned",
              "createdAtUtc": "2026-09-17T07:00:00+00:00"
            }
            """;
        var handler = new StubHttpMessageHandler(json);
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        var client = new MediaApiClient(httpClient);

        var result = await client.CreateMediaAsync(
            new CreateMediaRequest(
                "Berserk",
                MediaType.Anime,
                "Anime",
                "tmdb",
                "12345"));

        Assert.Equal(HttpMethod.Post, handler.LastMethod);
        Assert.Equal("http://localhost/api/media", handler.LastRequestUri?.ToString());
        Assert.Contains("\"type\":\"Anime\"", handler.LastRequestBody);
        Assert.Equal("Berserk", result.Title);
        Assert.Equal(MediaType.Manga, result.Type);
        Assert.Contains("\"externalSource\":\"tmdb\"", handler.LastRequestBody);
    }

    [Fact]
    public async Task SearchMediaAsync_EncodesQueryAndDeserializesResults()
    {
        const string json = """
            [{
              "externalSource": "tmdb",
              "externalId": "438631",
              "title": "Dune",
              "type": "Movie",
              "communityRating": 7.8,
              "releaseDate": "2021-09-15",
              "cast": ["Timothee Chalamet", "Rebecca Ferguson"]
            }]
            """;
        var handler = new StubHttpMessageHandler(json);
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        var client = new MediaApiClient(httpClient);

        var results = await client.SearchMediaAsync("Dune part two");

        var result = Assert.Single(results);
        Assert.Equal("Dune", result.Title);
        Assert.Equal(7.8m, result.CommunityRating);
        Assert.Equal(["Timothee Chalamet", "Rebecca Ferguson"], result.Cast);
        Assert.Equal(
            "http://localhost/api/media/search?query=Dune%20part%20two",
            handler.LastRequestUri?.OriginalString);
    }

    [Fact]
    public async Task SearchMediaAsync_SendsDiscoveryFilters()
    {
        var handler = new StubHttpMessageHandler("[]");
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        var client = new MediaApiClient(httpClient);

        await client.SearchMediaAsync(null, MediaType.Movie, 18, 1999, "Tom Hanks");

        Assert.Equal(
            "http://localhost/api/media/search?type=Movie&genreId=18&year=1999&actor=Tom%20Hanks",
            handler.LastRequestUri?.OriginalString);
    }

    private sealed class StubHttpMessageHandler(string content) : HttpMessageHandler
    {
        public HttpMethod? LastMethod { get; private set; }
        public Uri? LastRequestUri { get; private set; }
        public string? LastRequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastMethod = request.Method;
            LastRequestUri = request.RequestUri;
            LastRequestBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };

            return response;
        }
    }
}
