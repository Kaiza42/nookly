using System.Net.Http;
using System.Net.Http.Headers;

namespace Nookly.Desktop.Services;

public sealed class AuthenticatedHttpHandler(SessionStore sessionStore) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (sessionStore.AccessToken is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", sessionStore.AccessToken);
        return base.SendAsync(request, cancellationToken);
    }
}
