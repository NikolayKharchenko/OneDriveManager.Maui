using Microsoft.Identity.Client;

namespace OneDriveAlbums.UI;

public sealed partial class MsalAuthProvider
{
    static private partial string GetRedirectUri(string clientId) => $"msal{clientId}://auth";

    static private partial PublicClientApplicationBuilder ConfigurePlatform(PublicClientApplicationBuilder builder) => builder;

    static private partial AcquireTokenInteractiveParameterBuilder ConfigureInteractive(AcquireTokenInteractiveParameterBuilder builder)
        => builder.WithParentActivityOrWindow(() => Microsoft.Maui.ApplicationModel.Platform.CurrentActivity);
}