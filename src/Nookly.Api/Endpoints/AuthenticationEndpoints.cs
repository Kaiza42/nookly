using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nookly.Api.Authentication;
using Nookly.Api.Email;
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
        group.MapGet("/confirm-email", ConfirmEmailAsync).AllowAnonymous();
        group.MapPost("/forgot-password", ForgotPasswordAsync).AllowAnonymous();
        group.MapPost("/reset-password", ResetPasswordAsync).AllowAnonymous();
        group.MapGet("/me", (Nookly.Application.Abstractions.ICurrentMember current, NooklyDbContext db, CancellationToken token) =>
            db.Members.Where(member => member.Id == current.Id)
                .Select(member => new MemberResponse(member.Id, member.Email, member.DisplayName,
                    member.Role.ToString(), member.IsEmailConfirmed, member.PublicId))
                .SingleAsync(token)).RequireAuthorization();
        return endpoints;
    }

    private static async Task<IResult> RegisterAsync(RegisterRequest request, NooklyDbContext db,
        IPasswordHasher<Member> hasher, AccountTokenService tokens, IEmailSender emailSender,
        IConfiguration configuration, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (email == "emerick.roeting1@gmail.com") return Results.Forbid();
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
        var confirmation = tokens.Create(member.Id, AccountTokenPurpose.ConfirmEmail, TimeSpan.FromHours(24));
        db.AccountTokens.Add(confirmation.Token);
        await db.SaveChangesAsync(cancellationToken);
        var baseUrl = (configuration["NOOKLY_PUBLIC_API_URL"] ?? "http://localhost:5186").TrimEnd('/');
        await emailSender.SendAsync(member.Email, "Confirme ton inscription Nookly",
            $"Bienvenue {member.DisplayName}. Confirme ton adresse avec ce lien :\n{baseUrl}/api/auth/confirm-email?token={confirmation.Raw}", cancellationToken);
        return Results.Accepted(value: new MessageResponse("Compte cree. Consulte ton email pour confirmer ton inscription."));
    }

    private static async Task<IResult> LoginAsync(LoginRequest request, NooklyDbContext db,
        IPasswordHasher<Member> hasher, JwtTokenService tokens, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var member = await db.Members.SingleOrDefaultAsync(item => item.Email == email, cancellationToken);
        if (member is null || hasher.VerifyHashedPassword(member, member.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            return Results.Unauthorized();
        if (!member.IsEmailConfirmed) return Results.Json(new { error = "Confirme d'abord ton adresse email." }, statusCode: StatusCodes.Status403Forbidden);
        return Results.Ok(tokens.Create(member, request.StaySignedIn));
    }

    private static async Task<IResult> ConfirmEmailAsync(string token, NooklyDbContext db, CancellationToken cancellationToken)
    {
        var hash = AccountTokenService.Hash(token);
        var accountToken = await db.AccountTokens.SingleOrDefaultAsync(x => x.TokenHash == hash && x.Purpose == AccountTokenPurpose.ConfirmEmail, cancellationToken);
        if (accountToken is null || !accountToken.CanUse(DateTimeOffset.UtcNow)) return Results.BadRequest("Lien invalide ou expire.");
        var member = await db.Members.SingleAsync(x => x.Id == accountToken.MemberId, cancellationToken);
        member.ConfirmEmail(); accountToken.MarkUsed(DateTimeOffset.UtcNow); await db.SaveChangesAsync(cancellationToken);
        return Results.Content("<html><body style='font-family:Segoe UI;padding:40px'><h1>Email confirme</h1><p>Tu peux maintenant te connecter a Nookly.</p></body></html>", "text/html");
    }

    private static async Task<IResult> ForgotPasswordAsync(ForgotPasswordRequest request, NooklyDbContext db,
        AccountTokenService tokens, IEmailSender emailSender, IConfiguration configuration, CancellationToken cancellationToken)
    {
        var member = await db.Members.SingleOrDefaultAsync(x => x.Email == request.Email.Trim().ToLowerInvariant(), cancellationToken);
        if (member is not null)
        {
            var reset = tokens.Create(member.Id, AccountTokenPurpose.ResetPassword, TimeSpan.FromMinutes(30));
            db.AccountTokens.Add(reset.Token); await db.SaveChangesAsync(cancellationToken);
            await emailSender.SendAsync(member.Email, "Reinitialisation du mot de passe Nookly",
                $"Ton code de reinitialisation valable 30 minutes :\n{reset.Raw}", cancellationToken);
        }
        return Results.Ok(new MessageResponse("Si cette adresse existe, un email vient d'etre envoye."));
    }

    private static async Task<IResult> ResetPasswordAsync(ResetPasswordRequest request, NooklyDbContext db,
        IPasswordHasher<Member> hasher, CancellationToken cancellationToken)
    {
        if (request.NewPassword.Length < 10) return Results.BadRequest(new { error = "Le mot de passe doit contenir au moins 10 caracteres." });
        var hash = AccountTokenService.Hash(request.Token.Trim());
        var reset = await db.AccountTokens.SingleOrDefaultAsync(x => x.TokenHash == hash && x.Purpose == AccountTokenPurpose.ResetPassword, cancellationToken);
        if (reset is null || !reset.CanUse(DateTimeOffset.UtcNow)) return Results.BadRequest(new { error = "Code invalide ou expire." });
        var member = await db.Members.SingleAsync(x => x.Id == reset.MemberId, cancellationToken);
        member.SetPasswordHash(hasher.HashPassword(member, request.NewPassword)); reset.MarkUsed(DateTimeOffset.UtcNow);
        await db.SaveChangesAsync(cancellationToken); return Results.Ok(new MessageResponse("Mot de passe modifie."));
    }
}
