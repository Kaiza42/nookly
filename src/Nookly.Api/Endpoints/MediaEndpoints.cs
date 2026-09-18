using Nookly.Application.Abstractions;
using Nookly.Application.Media;
using Nookly.Application.Discovery;
using Nookly.Contracts.Discovery;
using Nookly.Contracts.Media;
using Nookly.Contracts.Search;
using Nookly.Contracts.Details;
using ApplicationCreateMediaRequest = Nookly.Application.Media.CreateMediaRequest;
using ApplicationUpdateMediaRequest = Nookly.Application.Media.UpdateMediaRequest;
using ContractCreateMediaRequest = Nookly.Contracts.Media.CreateMediaRequest;
using ContractUpdateMediaRequest = Nookly.Contracts.Media.UpdateMediaRequest;
using ContractMediaStatus = Nookly.Contracts.Media.MediaStatus;
using ContractMediaType = Nookly.Contracts.Media.MediaType;

namespace Nookly.Api.Endpoints;

public static class MediaEndpoints
{
    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/media").WithTags("Media").RequireAuthorization("MemberOnly");

        group.MapGet("/", async (IMediaService service, CancellationToken cancellationToken) =>
        {
            var items = await service.ListAsync(cancellationToken);
            return Results.Ok(items.Select(ToResponse));
        });

        group.MapGet("/search", async (
            string? query,
            ContractMediaType? type,
            int? genreId,
            int? year,
            string? actor,
            IExternalMediaSearch search,
            DiscoveryPreferenceService preferences,
            IMediaService mediaService,
            CancellationToken cancellationToken) =>
        {
            if (year is < 1900 or > 2100)
            {
                return Results.BadRequest(new { error = "The year is invalid." });
            }

            try
            {
                var isUnfilteredDiscovery = string.IsNullOrWhiteSpace(query) && type is null &&
                                            genreId is null && year is null &&
                                            string.IsNullOrWhiteSpace(actor);
                IReadOnlyList<Nookly.Application.Search.MediaSearchResult> results;
                if (isUnfilteredDiscovery)
                {
                    var seeds = await preferences.GetRecommendationSeedsAsync("tmdb", cancellationToken);
                    results = seeds.Count > 0
                        ? await search.RecommendAsync(seeds, cancellationToken)
                        : await search.SearchAsync(null, cancellationToken: cancellationToken);
                }
                else
                {
                    results = await search.SearchAsync(
                        query,
                        type is null ? null : (Nookly.Domain.Media.MediaType)type,
                        genreId,
                        year,
                        actor,
                        cancellationToken);
                }
                var dislikedIds = await preferences.GetDislikedIdsAsync("tmdb", cancellationToken);
                var libraryItems = await mediaService.ListAsync(cancellationToken);
                var libraryIds = libraryItems
                    .Where(item => item.ExternalSource == "tmdb" && item.ExternalId is not null)
                    .Select(item => item.ExternalId!)
                    .ToHashSet(StringComparer.Ordinal);
                return Results.Ok(results
                    .Where(item => !dislikedIds.Contains(item.ExternalId) &&
                                   !libraryIds.Contains(item.ExternalId))
                    .Select(item => new MediaSearchResultResponse(
                    item.ExternalSource,
                    item.ExternalId,
                    item.Title,
                    item.Description,
                    (ContractMediaType)item.Type,
                    item.PosterUrl,
                    item.CommunityRating,
                    item.ReleaseDate,
                    item.Cast)));
            }
            catch (InvalidOperationException exception)
            {
                return Results.Problem(exception.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
            }
            catch (HttpRequestException)
            {
                return Results.Problem(
                    "TMDB is temporarily unavailable.",
                    statusCode: StatusCodes.Status502BadGateway);
            }
        });

        group.MapGet("/external/{type}/{externalId}", async (
            ContractMediaType type,
            string externalId,
            IExternalMediaSearch search,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var details = await search.GetDetailsAsync(
                    externalId,
                    (Nookly.Domain.Media.MediaType)type,
                    cancellationToken);
                return details is null ? Results.NotFound() : Results.Ok(ToDetailsResponse(details));
            }
            catch (InvalidOperationException exception)
            {
                return Results.Problem(exception.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
            }
            catch (HttpRequestException)
            {
                return Results.Problem("TMDB is temporarily unavailable.", statusCode: StatusCodes.Status502BadGateway);
            }
        });

        group.MapGet("/external/{externalId}/seasons/{seasonNumber:int}", async (
            string externalId,
            int seasonNumber,
            IExternalMediaSearch search,
            CancellationToken cancellationToken) =>
        {
            if (seasonNumber < 1) return Results.BadRequest();
            try
            {
                var season = await search.GetSeasonAsync(externalId, seasonNumber, cancellationToken);
                return season is null ? Results.NotFound() : Results.Ok(new SeasonDetailsResponse(
                    season.Number,
                    season.Title,
                    season.Description,
                    season.PosterUrl,
                    season.AirDate,
                    season.CommunityRating,
                    season.Episodes.Select(episode => new EpisodeResponse(
                        episode.Number,
                        episode.Title,
                        episode.Description,
                        episode.ImageUrl,
                        episode.AirDate,
                        episode.RuntimeMinutes,
                        episode.CommunityRating)).ToArray()));
            }
            catch (HttpRequestException)
            {
                return Results.Problem("TMDB is temporarily unavailable.", statusCode: StatusCodes.Status502BadGateway);
            }
        });

        group.MapPost("/preferences", async (
            SetDiscoveryPreferenceRequest request,
            DiscoveryPreferenceService preferences,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.ExternalSource) ||
                string.IsNullOrWhiteSpace(request.ExternalId))
            {
                return Results.BadRequest();
            }

            await preferences.SaveAsync(
                request.ExternalSource,
                request.ExternalId,
                request.Title,
                request.IsLiked,
                (Nookly.Domain.Media.MediaType)request.Type,
                cancellationToken);
            return Results.NoContent();
        });

        group.MapGet("/preferences/disliked", async (
            DiscoveryPreferenceService preferences,
            CancellationToken cancellationToken) =>
        {
            var items = await preferences.ListDislikedAsync(cancellationToken);
            return Results.Ok(items.Select(item => new DiscoveryPreferenceResponse(
                item.ExternalSource,
                item.ExternalId,
                item.Title,
                item.MediaType is null ? null : (ContractMediaType)item.MediaType,
                item.IsLiked,
                item.Description,
                item.PosterUrl,
                item.CommunityRating,
                item.ReleaseDate,
                item.Cast)));
        });

        group.MapDelete("/preferences/{source}/{externalId}", async (
            string source,
            string externalId,
            DiscoveryPreferenceService preferences,
            CancellationToken cancellationToken) =>
        {
            var restored = await preferences.RestoreAsync(source, externalId, cancellationToken);
            return restored ? Results.NoContent() : Results.NotFound();
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            IMediaService service,
            CancellationToken cancellationToken) =>
        {
            var mediaItem = await service.GetByIdAsync(id, cancellationToken);
            return mediaItem is null ? Results.NotFound() : Results.Ok(ToResponse(mediaItem));
        }).WithName("GetMediaById");

        group.MapPost("/", async (
            ContractCreateMediaRequest request,
            IMediaService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var command = new ApplicationCreateMediaRequest(
                    request.Title,
                    (Nookly.Domain.Media.MediaType)request.Type,
                    request.Description,
                    request.ExternalSource,
                    request.ExternalId,
                    request.PosterUrl,
                    request.CommunityRating,
                    request.ReleaseDate);

                var mediaItem = await service.CreateAsync(command, cancellationToken);
                return Results.CreatedAtRoute(
                    "GetMediaById",
                    new { id = mediaItem.Id },
                    ToResponse(mediaItem));
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { error = exception.Message });
            }
            catch (DuplicateMediaException exception)
            {
                return Results.Conflict(new { error = exception.Message });
            }
        });

        group.MapPut("/{id:guid}", async (
            Guid id,
            ContractUpdateMediaRequest request,
            IMediaService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var command = new ApplicationUpdateMediaRequest(
                    (Nookly.Domain.Media.MediaStatus)request.Status,
                    request.PersonalRating,
                    request.PersonalNotes,
                    request.IsFavorite,
                    request.CurrentSeason,
                    request.CurrentEpisode);
                var mediaItem = await service.UpdateAsync(id, command, cancellationToken);
                return mediaItem is null ? Results.NotFound() : Results.Ok(ToResponse(mediaItem));
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { error = exception.Message });
            }
        });

        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMediaService service,
            CancellationToken cancellationToken) =>
        {
            var deleted = await service.DeleteAsync(id, cancellationToken);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        return endpoints;
    }

    private static MediaItemResponse ToResponse(MediaItemDto item) => new(
        item.Id,
        item.Title,
        item.Description,
        (ContractMediaType)item.Type,
        (ContractMediaStatus)item.Status,
        item.PersonalRating,
        item.PersonalNotes,
        item.IsFavorite,
        item.CurrentSeason,
        item.CurrentEpisode,
        item.ExternalSource,
        item.ExternalId,
        item.PosterUrl,
        item.CommunityRating,
        item.ReleaseDate,
        item.CreatedAtUtc,
        item.UpdatedAtUtc);

    private static MediaDetailsResponse ToDetailsResponse(Nookly.Application.Details.MediaDetails details) => new(
        details.ExternalId,
        details.Title,
        (ContractMediaType)details.Type,
        details.Description,
        details.PosterUrl,
        details.BackdropUrl,
        details.ReleaseDate,
        details.AirStatus,
        details.RuntimeMinutes,
        details.CommunityRating,
        details.Genres,
        details.Directors,
        details.Cast,
        details.TrailerUrl,
        details.Seasons.Select(season => new SeasonSummaryResponse(
            season.Number,
            season.Title,
            season.Description,
            season.PosterUrl,
            season.AirDate,
            season.EpisodeCount,
            season.CommunityRating)).ToArray());
}
