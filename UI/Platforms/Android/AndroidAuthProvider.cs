using Android.App;
using Microsoft.Identity.Client;
using OneDriveAlbums.Graph;

namespace OneDriveAlbums.UI;

public sealed class AndroidAuthProvider : AuthProviderImplBase
{
    public AndroidAuthProvider(string clientId) : base(clientId) { }

    protected override AcquireTokenInteractiveParameterBuilder WithModification(AcquireTokenInteractiveParameterBuilder builder)
    {
        Activity activity = PlatformActivityProvider.GetCurrentActivity();
        return builder.WithParentActivityOrWindow(activity);
    }
}