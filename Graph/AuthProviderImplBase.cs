using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
#if WINDOWS
using Microsoft.Identity.Client.Desktop;
#endif

namespace OneDriveAlbums.Graph;

public abstract class AuthProviderImplBase : IAuthenticationProvider
{
    public IPublicClientApplication PCA;

    protected AuthProviderImplBase(string clientId)
    {
        PCA = PublicClientApplicationBuilder
            .Create(clientId)
            .WithAuthority(AzureCloudInstance.AzurePublic, AadAuthorityAudience.PersonalMicrosoftAccount)
            .WithRedirectUri("http://localhost")
#if WINDOWS
            .WithWindowsEmbeddedBrowserSupport()
#endif
            .Build();
    }

    public async Task AuthenticateRequestAsync(
            RequestInformation request,
            Dictionary<string, object>? additionalAuthenticationContext = null,
            CancellationToken cancellationToken = default)
    {
        _ = additionalAuthenticationContext;

        string token = await getAccessTokenAsync(cancellationToken);
        request.Headers.TryAdd("Authorization", $"Bearer {token}");
    }

    private async Task<string> getAccessTokenAsync(CancellationToken cancellationToken)
    {
        IAccount? account = (await PCA.GetAccountsAsync()).FirstOrDefault();

        if (account != null)
        {
            try
            {
                var silent = await PCA.AcquireTokenSilent(GraphClient.Scopes, account)
                    .ExecuteAsync(cancellationToken);

                return silent.AccessToken;
            }
            catch (MsalUiRequiredException)
            {
            }
        }

        return await GetAccessTokenInteractiveAsync(GraphClient.Scopes, cancellationToken);
    }

    protected abstract Task<string> GetAccessTokenInteractiveAsync(string[] scopes, CancellationToken cancellationToken);
}
