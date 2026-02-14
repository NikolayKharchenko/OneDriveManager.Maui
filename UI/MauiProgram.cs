using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OneDriveAlbums.UI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        hookUnhandledExceptions();

        StartupLog.Write("MauiProgram:CreateMauiApp begin");

        StartupLog.Write($"FrameworkDescription: {RuntimeInformation.FrameworkDescription}");
        StartupLog.Write($"Environment.Version: {Environment.Version}");

        var enableDynamicCode = AppContext.GetData("System.Reflection.EnableDynamicCode");
        var disableDynamicInvoke = AppContext.GetData("System.Reflection.EmitDisableDynamicInvoke");

        StartupLog.Write($"System.Reflection.EnableDynamicCode (AppContext): {(enableDynamicCode is null ? "<null>" : enableDynamicCode)}");
        StartupLog.Write($"System.Reflection.EmitDisableDynamicInvoke (AppContext): {(disableDynamicInvoke is null ? "<null>" : disableDynamicInvoke)}");

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialSymbolsRounded.ttf", "MaterialSymbolsRounded");
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
