using System.Security.Claims;
using Nookly.Application.Abstractions;

namespace Nookly.Api.Authentication;

public sealed class CurrentMember(IHttpContextAccessor accessor) : ICurrentMember
{
    public Guid Id => Guid.TryParse(
        accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
        ? id
        : throw new UnauthorizedAccessException("No authenticated member.");
}
