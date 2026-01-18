namespace OneDriveAlbums.Graph;

public interface IAuthProvider
{
    Task<string> GetAccessTokenAsync(string[] scopes, CancellationToken cancellationToken = default);
}