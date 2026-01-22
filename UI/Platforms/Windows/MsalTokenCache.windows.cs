using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;

namespace OneDriveAlbums.UI;

internal static class MsalTokenCache
{
    static MsalCacheHelper? cacheHelper;

    const string CacheFileName = "msal_cache.bin3";
    static readonly string CacheDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "OneDriveAlbums");

    internal static async Task EnableAsync(IPublicClientApplication pca)
    {
        Directory.CreateDirectory(CacheDirectory);

        var storageProps = new StorageCreationPropertiesBuilder(
                cacheFileName: CacheFileName,
                cacheDirectory: CacheDirectory)
            .WithUnprotectedFile()
            .Build();

        cacheHelper = await MsalCacheHelper.CreateAsync(storageProps).ConfigureAwait(false);
        cacheHelper.RegisterCache(pca.UserTokenCache);
    }

    internal static void Disable(IPublicClientApplication pca)
    {
        if (cacheHelper != null)
        {
            cacheHelper.UnregisterCache(pca.UserTokenCache);
        }

        string fullPath = Path.Combine(CacheDirectory, CacheFileName);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}
