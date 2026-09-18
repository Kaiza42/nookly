using System.Security.Cryptography;
using System.Text;
using Nookly.Domain.Members;

namespace Nookly.Api.Authentication;

public sealed class AccountTokenService
{
    public (string Raw, AccountToken Token) Create(Guid memberId, AccountTokenPurpose purpose, TimeSpan lifetime)
    {
        var raw = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        return (raw, AccountToken.Create(memberId, Hash(raw), purpose, DateTimeOffset.UtcNow.Add(lifetime)));
    }
    public static string Hash(string raw) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
}
