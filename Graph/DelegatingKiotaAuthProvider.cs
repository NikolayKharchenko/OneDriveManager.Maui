using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace OneDriveManager.Graph;

internal sealed class DelegatingKiotaAuthProvider(IAuthProvider auth, string[] scopes) : IAuthenticationProvider
{
    public async Task AuthenticateRequestAsync(
        RequestInformation request,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        _ = additionalAuthenticationContext;

        string token = await auth.GetAccessTokenAsync(scopes, cancellationToken).ConfigureAwait(false);
        request.Headers.TryAdd("Authorization", $"Bearer {token}");
    }
}
