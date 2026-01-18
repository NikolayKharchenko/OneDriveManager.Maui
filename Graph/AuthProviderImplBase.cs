using Microsoft.Identity.Client;

namespace OneDriveAlbums.Graph;

public abstract class AuthProviderImplBase : IAuthProvider
{
    protected IPublicClientApplication? pca;
    string clientId;

    protected AuthProviderImplBase(string clientId)
    {
        this.clientId = clientId;
    }
        
    public async Task<string> GetAccessTokenAsync(string[] scopes, CancellationToken cancellationToken)
    {
        pca = PublicClientApplicationBuilder
            .Create(clientId)
            .WithAuthority(AzureCloudInstance.AzurePublic, AadAuthorityAudience.PersonalMicrosoftAccount)
            // IMPORTANT: don't force http://localhost; let MSAL pick the loopback redirect + port.
            .WithRedirectUri("http://localhost")
            .Build();
        await MsalTokenCache.InitializeAsync(pca);
        IAccount? account = (await pca.GetAccountsAsync()).FirstOrDefault();

        if (account != null)
        {
            try
            {
                var silent = await pca.AcquireTokenSilent(scopes, account)
                    .ExecuteAsync(cancellationToken);

                return silent.AccessToken;
            }
            catch (MsalUiRequiredException)
            {
            }
        }

        return await GetAccessTokenInteractiveAsync(scopes, cancellationToken).ConfigureAwait(false);
    }

    protected abstract Task<string> GetAccessTokenInteractiveAsync(string[] scopes, CancellationToken cancellationToken);
}
