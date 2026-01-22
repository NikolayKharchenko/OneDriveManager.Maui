using Microsoft.Identity.Client;
using Microsoft.Maui.ApplicationModel;

namespace OneDriveAlbums.UI;

public sealed partial class MsalAuthProvider
{
    static private partial string GetRedirectUri(string clientId) => $"msal{clientId}://auth";

    static private partial PublicClientApplicationBuilder ConfigurePlatform(PublicClientApplicationBuilder builder) 
        => builder.WithParentActivityOrWindow(() => Platform.CurrentActivity!);

    static private partial AcquireTokenInteractiveParameterBuilder ConfigureInteractive(AcquireTokenInteractiveParameterBuilder builder)
        //=> builder.WithParentActivityOrWindow(() => Platform.CurrentActivity!);
        => builder;
}