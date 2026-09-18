using Microsoft.EntityFrameworkCore;
using Nookly.Application.Abstractions;
using Nookly.Contracts.Administration;
using Nookly.Domain.Members;
using Nookly.Infrastructure.Data;

namespace Nookly.Api.Endpoints;

public static class AdministrationEndpoints
{
    public static IEndpointRouteBuilder MapAdministrationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/admin").WithTags("Administration").RequireAuthorization("AdminOnly");
        group.MapGet("/members", async (NooklyDbContext db, CancellationToken token) =>
            await db.Members.AsNoTracking().Where(x => x.Role == MemberRole.Member).OrderByDescending(x => x.CreatedAtUtc)
                .Select(x => new AdminMemberResponse(x.Id, x.Email, x.DisplayName, x.IsEmailConfirmed,
                    x.UsageSeconds, x.LastActivityAtUtc, x.CreatedAtUtc)).ToListAsync(token));
        group.MapPost("/notifications", async (SendNotificationRequest request, NooklyDbContext db, CancellationToken token) =>
        {
            if (!await db.Members.AnyAsync(x => x.Id == request.MemberId && x.Role == MemberRole.Member, token)) return Results.NotFound();
            try { db.MemberNotifications.Add(MemberNotification.Create(request.MemberId, request.Title, request.Message)); }
            catch (ArgumentException ex) { return Results.BadRequest(new { error = ex.Message }); }
            await db.SaveChangesAsync(token); return Results.NoContent();
        });

        var member = endpoints.MapGroup("/api/member").RequireAuthorization("MemberOnly");
        member.MapPost("/activity", async (ICurrentMember current, NooklyDbContext db, CancellationToken token) =>
        { var entity = await db.Members.SingleAsync(x => x.Id == current.Id, token); entity.RecordActivity(DateTimeOffset.UtcNow); await db.SaveChangesAsync(token); return Results.NoContent(); });
        member.MapGet("/notifications", async (ICurrentMember current, NooklyDbContext db, CancellationToken token) =>
            await db.MemberNotifications.Where(x => x.MemberId == current.Id && x.ReadAtUtc == null).OrderBy(x => x.CreatedAtUtc)
                .Select(x => new NotificationResponse(x.Id, x.Title, x.Message, x.CreatedAtUtc)).ToListAsync(token));
        member.MapPost("/notifications/{id:guid}/read", async (Guid id, ICurrentMember current, NooklyDbContext db, CancellationToken token) =>
        { var item = await db.MemberNotifications.SingleOrDefaultAsync(x => x.Id == id && x.MemberId == current.Id, token); if (item is null) return Results.NotFound(); item.MarkRead(); await db.SaveChangesAsync(token); return Results.NoContent(); });
        return endpoints;
    }
}
