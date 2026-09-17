using Nookly.Application.Media;
using Nookly.Contracts.Media;
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
                    request.Description);

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
                    request.Title,
                    (Nookly.Domain.Media.MediaType)request.Type,
                    request.Description,
                    (Nookly.Domain.Media.MediaStatus)request.Status,
                    request.PersonalRating);
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
        item.CreatedAtUtc,
        item.UpdatedAtUtc);
}
