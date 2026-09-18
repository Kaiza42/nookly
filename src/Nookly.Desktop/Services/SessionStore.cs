using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Text.Json;
using Nookly.Contracts.Authentication;

namespace Nookly.Desktop.Services;

public sealed class SessionStore
{
    private readonly string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Nookly");
    public string? AccessToken { get; private set; }
    public MemberResponse? Member { get; private set; }
    public string RememberedEmail { get; private set; } = string.Empty;
    public bool StaySignedIn { get; private set; }

    public void Load()
    {
        Directory.CreateDirectory(directory);
        var emailPath = Path.Combine(directory, "email.txt");
        if (File.Exists(emailPath)) RememberedEmail = File.ReadAllText(emailPath).Trim();
        var sessionPath = Path.Combine(directory, "session.dat");
        if (!File.Exists(sessionPath)) return;
        try
        {
            var json = Encoding.UTF8.GetString(ProtectedData.Unprotect(File.ReadAllBytes(sessionPath), null, DataProtectionScope.CurrentUser));
            var saved = JsonSerializer.Deserialize<SavedSession>(json);
            if (saved is not null && saved.Version == 1 && saved.ExpiresAtUtc > DateTimeOffset.UtcNow)
            {
                AccessToken = saved.AccessToken;
                Member = saved.Member;
                StaySignedIn = true;
            }
        }
        catch (CryptographicException) { ClearToken(); }
    }

    public void Set(AuthenticationResponse response, bool staySignedIn)
    {
        AccessToken = response.AccessToken;
        Member = response.Member;
        StaySignedIn = staySignedIn;
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "email.txt"), response.Member.Email);
        RememberedEmail = response.Member.Email;
        if (staySignedIn)
        {
            var json = JsonSerializer.Serialize(new SavedSession(response.AccessToken, response.ExpiresAtUtc, response.Member, 1));
            File.WriteAllBytes(Path.Combine(directory, "session.dat"), ProtectedData.Protect(Encoding.UTF8.GetBytes(json), null, DataProtectionScope.CurrentUser));
        }
        else ClearSavedToken();
    }

    public void SetStaySignedIn(bool value)
    {
        StaySignedIn = value;
        if (!value) ClearSavedToken();
    }

    public void ClearToken() { AccessToken = null; Member = null; StaySignedIn = false; ClearSavedToken(); }
    private void ClearSavedToken() { var path = Path.Combine(directory, "session.dat"); if (File.Exists(path)) File.Delete(path); }
    private sealed record SavedSession(string AccessToken, DateTimeOffset ExpiresAtUtc, MemberResponse Member, int Version = 0);
}
