using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace OneDriveAlbums.UI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        HookUnhandledExceptions();

        MauiAppBuilder builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialSymbolsRounded.ttf", "MaterialSymbolsRounded");
                fonts.AddFont("FontAwesomeSolid.otf", "FontAwesomeSolid");
                fonts.AddFont("FontAwesomeRegular.otf", "FontAwesomeRegular");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static void HookUnhandledExceptions()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            var ex = e.ExceptionObject as Exception;
            Trace.WriteLine($"[UnhandledException][IsTerminating={e.IsTerminating}] {ex}");
            MainPage.Instance?.SetStatusText(ex?.Message!);
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Trace.WriteLine($"[UnobservedTaskException] {e.Exception}");
            e.SetObserved(); // prevents the process from being torn down later due to this exception
            MainPage.Instance?.SetStatusText(e.Exception.Message);
        };
    }
}
