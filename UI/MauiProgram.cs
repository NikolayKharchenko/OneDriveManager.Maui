using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Handlers;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace OneDriveAlbums.UI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        hookUnhandledExceptions();

#if IOS
        // iOS is AOT-only. Ensure reflection invocation never falls back to DynamicMethod / JIT.
        // This must be set before any code paths that may use reflection-based activation.
        AppContext.SetSwitch("System.Reflection.EmitDisableDynamicInvoke", true);
        AppContext.SetSwitch("System.Reflection.EnableDynamicCode", false);
#endif

        StartupLog.Write("MauiProgram:CreateMauiApp begin");

        StartupLog.Write($"FrameworkDescription: {RuntimeInformation.FrameworkDescription}");
        StartupLog.Write($"Environment.Version: {Environment.Version}");

        if (AppContext.TryGetSwitch("System.Reflection.EnableDynamicCode", out var enableDynamicCode))
            StartupLog.Write($"System.Reflection.EnableDynamicCode (switch): {enableDynamicCode}");
        else
            StartupLog.Write("System.Reflection.EnableDynamicCode (switch): <not set>");

        if (AppContext.TryGetSwitch("System.Reflection.EmitDisableDynamicInvoke", out var disableDynamicInvoke))
            StartupLog.Write($"System.Reflection.EmitDisableDynamicInvoke (switch): {disableDynamicInvoke}");
        else
            StartupLog.Write("System.Reflection.EmitDisableDynamicInvoke (switch): <not set>");

        StartupLog.Write($"RuntimeFeature.IsDynamicCodeSupported: {RuntimeFeature.IsDynamicCodeSupported}");
        StartupLog.Write($"RuntimeFeature.IsDynamicCodeCompiled: {RuntimeFeature.IsDynamicCodeCompiled}");

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialSymbolsRounded.ttf", "MaterialSymbolsRounded");
            });

#if IOS
        // Workaround for iOS AOT-only: avoid Activator.CreateInstance for LayoutHandler.
        // In Release/AOT it can hit a DynamicMethod invoke stub and crash with ExecutionEngineException.
        for (var i = builder.Services.Count - 1; i >= 0; i--)
        {
            var d = builder.Services[i];
            if (d.ServiceType == typeof(LayoutHandler) || d.ImplementationType == typeof(LayoutHandler))
                builder.Services.RemoveAt(i);
        }
        builder.Services.AddTransient<LayoutHandler>(_ => new LayoutHandler());
#endif

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
