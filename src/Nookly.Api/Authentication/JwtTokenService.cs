using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Nookly.Contracts.Authentication;
using Nookly.Domain.Members;

namespace Nookly.Api.Authentication;

public sealed class JwtTokenService(IConfiguration configuration)
{
    public AuthenticationResponse Create(Member member, bool staySignedIn)
    {
        var expires = DateTimeOffset.UtcNow.Add(staySignedIn ? TimeSpan.FromDays(30) : TimeSpan.FromHours(8));
        var key = configuration["NOOKLY_JWT_KEY"]
                  ?? throw new InvalidOperationException("NOOKLY_JWT_KEY is missing.");
        var token = new JwtSecurityToken(
            claims:
            [
                new Claim(ClaimTypes.NameIdentifier, member.Id.ToString()),
                new Claim(ClaimTypes.Email, member.Email),
                new Claim(ClaimTypes.Name, member.DisplayName),
                new Claim(ClaimTypes.Role, member.Role.ToString())
            ],
            expires: expires.UtcDateTime,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256));
        return new AuthenticationResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            expires,
            new MemberResponse(member.Id, member.Email, member.DisplayName, member.Role.ToString(), member.IsEmailConfirmed));
    }
}
