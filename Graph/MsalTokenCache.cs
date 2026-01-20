using System.IO;
using Microsoft.Identity.Client;

#if WINDOWS
using Microsoft.Identity.Client.Extensions.Msal;
#endif

namespace OneDriveAlbums.Graph;

public static class MsalTokenCache
{
    private static bool _initialized;

#if WINDOWS
    private static readonly string CacheFileName = "msal_cache.bin3";
#endif

    public static async Task InitializeAsync(IPublicClientApplication pca)
    {
        if (_initialized)
            return;

#if WINDOWS
        StorageCreationProperties properties =
            new StorageCreationPropertiesBuilder(CacheFileName, MsalCacheHelper.UserRootDirectory)
                .Build();

        MsalCacheHelper helper = await MsalCacheHelper.CreateAsync(properties);
        helper.RegisterCache(pca.UserTokenCache);
#endif

        _initialized = true;
    }

    public static async Task ClearAsync(IPublicClientApplication pca)
    {
        // Clears MSAL cache on ALL platforms
        var accounts = await pca.GetAccountsAsync();
        foreach (var account in accounts)
            await pca.RemoveAsync(account);

#if WINDOWS
        var cachePath = Path.Combine(MsalCacheHelper.UserRootDirectory, CacheFileName);
        if (File.Exists(cachePath))
            File.Delete(cachePath);
#endif

        _initialized = false;
    }
}