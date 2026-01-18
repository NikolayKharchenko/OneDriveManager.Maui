using OneDriveAlbums.Graph;

namespace OneDriveAlbums.UI;

internal static class Platform
{
    public static AuthProviderImplBase CreateAuthProvider(string clientId)
    {
        throw new NotImplementedException("iOS AuthProviderFactory is not implemented yet.");
    }
    public static string GetOneDriveLocalDirectory()
    {
        throw new NotImplementedException("iOS GetOneDriveLocalDirectory is not implemented yet.");
    }
}
