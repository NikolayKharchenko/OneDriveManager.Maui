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

        var interactiveBuilder = pca.AcquireTokenInteractive(scopes);

        if (hwnd != nint.Zero)
            interactiveBuilder = interactiveBuilder.WithParentActivityOrWindow(hwnd);

        var interactive = await interactiveBuilder
            .ExecuteAsync(cancellationToken)
            .ConfigureAwait(false);

        return interactive.AccessToken;
    }

    private static nint GetMauiAppHwnd()
    {
        // Prefer the current MAUI window; fallback to the first available.
        var mauiWindow = Application.Current?.Windows.FirstOrDefault();
        var winuiWindow = mauiWindow?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;

        return winuiWindow is null ? nint.Zero : WindowNative.GetWindowHandle(winuiWindow);
    }
}
