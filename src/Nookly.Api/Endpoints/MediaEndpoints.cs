using Nookly.Application.Abstractions;
using Nookly.Application.Media;
using Nookly.Contracts.Media;
using Nookly.Contracts.Search;
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
        var group = endpoints.MapGroup("/api/media").WithTags("Media");

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
            CancellationToken cancellationToken) =>
        {
            if (year is < 1900 or > 2100)
            {
                return Results.BadRequest(new { error = "The year is invalid." });
            }

            try
            {
                var results = await search.SearchAsync(
                    query,
                    type is null ? null : (Nookly.Domain.Media.MediaType)type,
                    genreId,
                    year,
                    actor,
                    cancellationToken);
                return Results.Ok(results.Select(item => new MediaSearchResultResponse(
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
}
