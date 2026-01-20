using Microsoft.Identity.Client;
using OneDriveAlbums.Graph;
using WinRT.Interop;

namespace OneDriveAlbums.UI;

internal sealed class MsalDesktopAuthProvider : AuthProviderImplBase
{
    public MsalDesktopAuthProvider(string clientId) : base(clientId)
    {
    }

    protected override AcquireTokenInteractiveParameterBuilder WithModification(AcquireTokenInteractiveParameterBuilder builder)
    {
        nint hwnd = GetMauiAppHwnd();
        return builder.WithParentActivityOrWindow(hwnd);
    }

    private static nint GetMauiAppHwnd()
    {
        var mauiWindow = Application.Current?.Windows.FirstOrDefault();
        var winuiWindow = mauiWindow?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;

        return winuiWindow is null ? nint.Zero : WindowNative.GetWindowHandle(winuiWindow);
    }
}
