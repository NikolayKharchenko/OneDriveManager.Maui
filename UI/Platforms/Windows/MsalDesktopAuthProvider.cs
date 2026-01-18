using OneDriveAlbums.Graph;
using WinRT.Interop;

namespace OneDriveAlbums.UI;

internal sealed class MsalDesktopAuthProvider : AuthProviderImplBase
{
    public MsalDesktopAuthProvider(string clientId) : base(clientId)
    {
    }

    protected override async Task<string> GetAccessTokenInteractiveAsync(string[] scopes, CancellationToken cancellationToken)
    {
        nint hwnd = GetMauiAppHwnd();

        var result = await PCA.AcquireTokenInteractive(scopes)
            .WithUseEmbeddedWebView(true)
            .WithParentActivityOrWindow(hwnd)
            .ExecuteAsync(cancellationToken)
            .ConfigureAwait(false);

        return result.AccessToken;
    }

    private static nint GetMauiAppHwnd()
    {
        var mauiWindow = Application.Current?.Windows.FirstOrDefault();
        var winuiWindow = mauiWindow?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;

        return winuiWindow is null ? nint.Zero : WindowNative.GetWindowHandle(winuiWindow);
    }
}
