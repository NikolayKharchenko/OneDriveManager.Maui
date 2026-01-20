using Android.App;
using Android.OS;

namespace OneDriveAlbums.UI;

internal static class PlatformActivityProvider
{
    private static Activity? current;

    public static void Init(Android.App.Application application)
    {
        application.RegisterActivityLifecycleCallbacks(new Callbacks());
    }

    public static Activity GetCurrentActivity()
        => current ?? throw new InvalidOperationException("Current Activity is not available yet.");

    private sealed class Callbacks : Java.Lang.Object, Android.App.Application.IActivityLifecycleCallbacks
    {
        public void OnActivityCreated(Activity activity, Bundle? savedInstanceState) => current = activity;
        public void OnActivityResumed(Activity activity) => current = activity;

        public void OnActivityDestroyed(Activity activity) { }
        public void OnActivityPaused(Activity activity) { }
        public void OnActivitySaveInstanceState(Activity activity, Bundle outState) { }
        public void OnActivityStarted(Activity activity) { }
        public void OnActivityStopped(Activity activity) { }
    }
}
