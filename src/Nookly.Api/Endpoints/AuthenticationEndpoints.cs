using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nookly.Api.Authentication;
using Nookly.Contracts.Authentication;
using Nookly.Domain.Members;
using Nookly.Infrastructure.Data;

namespace Nookly.Api.Endpoints;

public static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth").WithTags("Authentication");
        group.MapPost("/register", RegisterAsync).AllowAnonymous();
        group.MapPost("/login", LoginAsync).AllowAnonymous();
        group.MapGet("/me", (Nookly.Application.Abstractions.ICurrentMember current, NooklyDbContext db, CancellationToken token) =>
            db.Members.Where(member => member.Id == current.Id)
                .Select(member => new MemberResponse(member.Id, member.Email, member.DisplayName))
                .SingleAsync(token)).RequireAuthorization();
        return endpoints;
    }

    private static async Task<IResult> RegisterAsync(RegisterRequest request, NooklyDbContext db,
        IPasswordHasher<Member> hasher, JwtTokenService tokens, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (!email.Contains('@') || request.Password.Length < 10 || string.IsNullOrWhiteSpace(request.DisplayName))
            return Results.BadRequest(new { error = "Email, pseudo et mot de passe de 10 caracteres minimum requis." });
        if (await db.Members.AnyAsync(member => member.Email == email, cancellationToken))
            return Results.Conflict(new { error = "Cette adresse email est deja utilisee." });

        var member = Member.Create(email, request.DisplayName);
        member.SetPasswordHash(hasher.HashPassword(member, request.Password));
        db.Members.Add(member);
        if (!await db.Members.AnyAsync(cancellationToken))
        {
            await db.MediaItems.Where(item => item.MemberId == null).ExecuteUpdateAsync(
                setters => setters.SetProperty(item => item.MemberId, member.Id), cancellationToken);
            await db.DiscoveryPreferences.Where(item => item.MemberId == null).ExecuteUpdateAsync(
                setters => setters.SetProperty(item => item.MemberId, member.Id), cancellationToken);
        }
        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(tokens.Create(member, request.StaySignedIn));
    }

    private static async Task<IResult> LoginAsync(LoginRequest request, NooklyDbContext db,
        IPasswordHasher<Member> hasher, JwtTokenService tokens, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var member = await db.Members.SingleOrDefaultAsync(item => item.Email == email, cancellationToken);
        if (member is null || hasher.VerifyHashedPassword(member, member.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            return Results.Unauthorized();
        return Results.Ok(tokens.Create(member, request.StaySignedIn));
    }
}
