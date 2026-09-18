using Nookly.Application.Banking;
using Nookly.Contracts.Banking;

namespace Nookly.Api.Endpoints;

public static class BankEndpoints
{
    public static IEndpointRouteBuilder MapBankEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/bank").WithTags("Bank").RequireAuthorization();
        group.MapGet("/", async (BankService service, CancellationToken token) => ToResponse(await service.GetAsync(token)));
        group.MapPut("/starting-balance", async (SetStartingBalanceRequest request, BankService service, CancellationToken token) =>
        {
            try { return Results.Ok(ToResponse(await service.SetStartingBalanceAsync(request.Amount, token))); }
            catch (ArgumentOutOfRangeException exception) { return Results.BadRequest(new { error = exception.Message }); }
        });
        group.MapPost("/entries", async (CreateBankEntryRequest request, BankService service, CancellationToken token) =>
        {
            try { var entry = await service.AddEntryAsync(request.Label, request.Amount, token); return Results.Ok(new BankEntryResponse(entry.Id, entry.Label, entry.Amount, entry.CreatedAtUtc)); }
            catch (ArgumentException exception) { return Results.BadRequest(new { error = exception.Message }); }
        });
        group.MapDelete("/entries/{id:guid}", async (Guid id, BankService service, CancellationToken token) =>
            await service.DeleteEntryAsync(id, token) ? Results.NoContent() : Results.NotFound());
        return endpoints;
    }

    private static BankSummaryResponse ToResponse(Nookly.Application.Banking.BankSummaryDto summary) => new(
        summary.StartingBalance,
        summary.CurrentBalance,
        summary.Entries.Select(entry => new BankEntryResponse(entry.Id, entry.Label, entry.Amount, entry.CreatedAtUtc)).ToArray());
}
