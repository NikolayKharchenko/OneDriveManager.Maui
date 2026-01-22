using Microsoft.Identity.Client;
using UIKit;

namespace OneDriveAlbums.UI;

public sealed partial class MsalAuthProvider
{
    // MSAL on iOS typically expects a scheme-based redirect as well.
    static private partial string GetRedirectUri(string clientId) => $"msal{clientId}://auth";

    static private partial PublicClientApplicationBuilder ConfigurePlatform(PublicClientApplicationBuilder builder) => builder;

    static private partial AcquireTokenInteractiveParameterBuilder ConfigureInteractive(AcquireTokenInteractiveParameterBuilder builder)
        => builder.WithParentActivityOrWindow(() =>
        {
                var windowScene = UIApplication.SharedApplication.ConnectedScenes
                .OfType<UIWindowScene>()
                .OrderByDescending(s => s.ActivationState == UISceneActivationState.ForegroundActive)
                .FirstOrDefault();

            var window = windowScene?.Windows?.FirstOrDefault(w => w.IsKeyWindow)
                ?? windowScene?.Windows?.FirstOrDefault();

            return window?.RootViewController
                ?? throw new InvalidOperationException("Unable to resolve a RootViewController for MSAL interactive auth.");
        });
}