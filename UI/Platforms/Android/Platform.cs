using OneDriveAlbums.Graph;

namespace OneDriveAlbums.UI;

internal static class Platform
{
    public static AuthProviderImplBase CreateAuthProvider(string clientId)
    {
        return new AndroidAuthProvider(clientId);
    }
}
