using OneDriveAlbums.Graph;
using OneDriveManager.Graph;

namespace OneDriveAlbums.UI;

internal static class Platform
{
    public static IAuthProvider CreateAuthProvider(string clientId)
    {
        return new MsalDesktopAuthProvider(clientId);
    }
    public static string GetOneDriveLocalDirectory()
    {
        string? oneDriveRoot = OneDriveFolderLocator.TryGetConsumerOneDriveRoot();
        return oneDriveRoot ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OneDrive");
    }
}
