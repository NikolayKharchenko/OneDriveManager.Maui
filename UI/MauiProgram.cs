using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using System.Diagnostics;

namespace OneDriveAlbums.UI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        hookUnhandledExceptions();

        StartupLog.Clear();
        StartupLog.Write("MauiProgram:CreateMauiApp begin");

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialSymbolsRounded.ttf", "MaterialSymbolsRounded");
            })
            .ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler(typeof(ContentView), typeof(ContentViewHandler));
                handlers.AddHandler(typeof(Layout), typeof(LayoutHandler));
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        StartupLog.Write("MauiProgram:CreateMauiApp end");
        return app;
    }

    private static void hookUnhandledExceptions()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            var ex = e.ExceptionObject as Exception;
            StartupLog.Write(ex ?? new Exception("Unknown exception"), $"[UnhandledException][IsTerminating={e.IsTerminating}]");
            Trace.WriteLine($"[UnhandledException][IsTerminating={e.IsTerminating}] {ex}");
            MainPage.Instance?.SetStatusText(ex?.Message!);
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            StartupLog.Write(e.Exception, "[UnobservedTaskException]");
            Trace.WriteLine($"[UnobservedTaskException] {e.Exception}");
            e.SetObserved();
            MainPage.Instance?.SetStatusText(e.Exception.Message);
        };
    }
}
