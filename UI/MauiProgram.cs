using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace OneDriveAlbums.UI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        HookUnhandledExceptions();

        StartupLog.Write("MauiProgram:CreateMauiApp begin");

        MauiAppBuilder builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialSymbolsRounded.ttf", "MaterialSymbolsRounded");
            });
        StartupLog.Write("MauiProgram:builder created");

#if DEBUG
        builder.Logging.AddDebug();
#endif

        try
        {
            StartupLog.Write("MauiProgram:builder.Build begin");
            var app = builder.Build();
            StartupLog.Write("MauiProgram:builder.Build end");
            StartupLog.Write("MauiProgram:CreateMauiApp end");
            return app;
        }
        catch (Exception ex)
        {
            StartupLog.Write(ex, "MauiProgram:builder.Build failed");
            LogInnerExceptions(ex);
            throw;
        }
    }

    private static void LogInnerExceptions(Exception ex)
    {
        int i = 0;
        for (Exception? cur = ex.InnerException; cur != null; cur = cur.InnerException)
        {
            StartupLog.Write(cur, $"MauiProgram:inner[{++i}]");
        }
    }

    private static void HookUnhandledExceptions()
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
