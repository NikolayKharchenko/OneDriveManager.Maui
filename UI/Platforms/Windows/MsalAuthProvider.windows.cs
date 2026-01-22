using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Desktop;

namespace OneDriveAlbums.UI;

public sealed partial class MsalAuthProvider
{
    static private partial string GetRedirectUri(string clientId) => "http://localhost";

    static private partial PublicClientApplicationBuilder ConfigurePlatform(PublicClientApplicationBuilder builder) => builder.WithWindowsEmbeddedBrowserSupport();

    static private partial AcquireTokenInteractiveParameterBuilder ConfigureInteractive(AcquireTokenInteractiveParameterBuilder builder) => builder;
}