using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using OneDriveAlbums.Graph;

namespace OneDriveAlbums.UI;

public sealed partial class MsalAuthProvider : IAuthenticationProvider
{
    private readonly IPublicClientApplication _pca;

    public MsalAuthProvider(string clientId)
    {
        var builder = PublicClientApplicationBuilder
            .Create(clientId)
            .WithAuthority(AzureCloudInstance.AzurePublic, AadAuthorityAudience.PersonalMicrosoftAccount)
            .WithRedirectUri(GetRedirectUri(clientId));

        builder = ConfigurePlatform(builder);
        _pca = builder.Build();

#if WINDOWS
        // Must be done once, before any MSAL calls (GetAccountsAsync/AcquireTokenSilent/AcquireTokenInteractive)
        MsalTokenCache.EnableAsync(_pca).GetAwaiter().GetResult();
#endif
    }

    public async Task AuthenticateRequestAsync(
        RequestInformation request,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        _ = additionalAuthenticationContext;

        string token = await GetAccessTokenAsync(cancellationToken);
        request.Headers.TryAdd("Authorization", $"Bearer {token}");
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        IAccount? account = (await _pca.GetAccountsAsync()).FirstOrDefault();

        if (account != null)
        {
            try
            {
                AuthenticationResult silent = await _pca
                    .AcquireTokenSilent(GraphClient.Scopes, account)
                    .ExecuteAsync(cancellationToken);

                return silent.AccessToken;
            }
            catch (MsalUiRequiredException)
            {
            }
        }

        AcquireTokenInteractiveParameterBuilder interactive = _pca
            .AcquireTokenInteractive(GraphClient.Scopes)
            .WithUseEmbeddedWebView(true);

        AuthenticationResult result = await interactive.ExecuteAsync(cancellationToken);
        return result.AccessToken;
    }


    public async Task ClearTokenCacheAsync()
    {
        var accounts = await _pca.GetAccountsAsync();
        foreach (var account in accounts)
        {
            await _pca.RemoveAsync(account);
        }
#if WINDOWS
        MsalTokenCache.Disable(_pca);
#endif
    }

    static private partial string GetRedirectUri(string clientId);
    static private partial PublicClientApplicationBuilder ConfigurePlatform(PublicClientApplicationBuilder builder);
}
