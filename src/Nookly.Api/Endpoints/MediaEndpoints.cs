using Nookly.Application.Media;

namespace Nookly.Api.Endpoints;

public static class MediaEndpoints
{
    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/media").WithTags("Media");

        group.MapGet("/", async (IMediaService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListAsync(cancellationToken)));

        group.MapGet("/{id:guid}", async (
            Guid id,
            IMediaService service,
            CancellationToken cancellationToken) =>
        {
            var mediaItem = await service.GetByIdAsync(id, cancellationToken);
            return mediaItem is null ? Results.NotFound() : Results.Ok(mediaItem);
        }).WithName("GetMediaById");

        group.MapPost("/", async (
            CreateMediaRequest request,
            IMediaService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var mediaItem = await service.CreateAsync(request, cancellationToken);
                return Results.CreatedAtRoute("GetMediaById", new { id = mediaItem.Id }, mediaItem);
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { error = exception.Message });
            }
        });

        return endpoints;
    }
}
