using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;

namespace OneDriveManager.Graph;

public static class MsalTokenCache
{
    private static bool _initialized;

    public static async Task InitializeAsync(IPublicClientApplication pca)
    {
        if (_initialized)
            return;

        StorageCreationProperties properties = new StorageCreationPropertiesBuilder("msal_cache.bin3", MsalCacheHelper.UserRootDirectory).Build();

        MsalCacheHelper helper = await MsalCacheHelper.CreateAsync(properties).ConfigureAwait(false);
        helper.RegisterCache(pca.UserTokenCache);

        _initialized = true;
    }
}