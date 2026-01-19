using System.IO;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;

namespace OneDriveAlbums.Graph;

public static class MsalTokenCache
{
    private static bool _initialized;
    static string CacheFileName = "msal_cache.bin3";

    public static async Task InitializeAsync(IPublicClientApplication pca)
    {
        if (_initialized)
            return;

        StorageCreationProperties properties =
            new StorageCreationPropertiesBuilder(CacheFileName, MsalCacheHelper.UserRootDirectory)
                .Build();

        MsalCacheHelper helper = await MsalCacheHelper.CreateAsync(properties);
        helper.RegisterCache(pca.UserTokenCache);

        _initialized = true;
    }

    public static async Task ClearAsync(IPublicClientApplication pca)
    {
        // 1) Clear in-memory MSAL state (accounts/refresh tokens)
        var accounts = await pca.GetAccountsAsync();
        foreach (var account in accounts)
        {
            await pca.RemoveAsync(account);
        }

        // 2) Remove the persisted cache file (what you registered via MsalCacheHelper)
        var cachePath = Path.Combine(MsalCacheHelper.UserRootDirectory, CacheFileName);
        if (File.Exists(cachePath))
        {
            File.Delete(cachePath);
        }

        // If you want the app to recreate and re-register the cache next time:
        _initialized = false;
    }
}