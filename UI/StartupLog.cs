using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OneDriveAlbums.UI;

internal static class StartupLog
{
    private const string LogFileName = "startup.log";
    private static readonly object Gate = new();

    private static string GetBaseDir()
    {
        try
        {
            var dir = FileSystem.AppDataDirectory;
            if (!string.IsNullOrWhiteSpace(dir))
                return dir;
        }
        catch { }

        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    }

    public static string LogPath => Path.Combine(GetBaseDir(), LogFileName);

    public static void Clear()
    {
        WriteCore("StartupLog:Clear begin");
        try
        {
            lock (Gate)
            {
                var baseDir = GetBaseDir();
                Directory.CreateDirectory(baseDir);
                File.WriteAllText(Path.Combine(baseDir, LogFileName), string.Empty);
            }
            WriteCore("StartupLog:Clear end");
        }
        catch (Exception ex)
        {
            WriteCore($"StartupLog:Clear failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    public static void Write(string message) => WriteCore(message);

    public static void Write(Exception ex, string message = "Exception")
        => WriteCore($"{message}: {ex}");

    private static void WriteCore(string message)
    {
        var line = $"{DateTimeOffset.UtcNow:O} [StartupLog] {message}";

        try { Trace.WriteLine(line); } catch { }

#if IOS
        try { IOSLog(line); } catch { }
#endif

        try
        {
            lock (Gate)
            {
                var baseDir = GetBaseDir();
                Directory.CreateDirectory(baseDir);
                File.AppendAllText(Path.Combine(baseDir, LogFileName), line + Environment.NewLine);
            }
        }
        catch { }
    }

#if IOS
    private static void IOSLog(string message)
        => os_log("%{public}s", message);

    [DllImport("/usr/lib/libSystem.dylib", EntryPoint = "os_log")]
    private static extern void os_log([MarshalAs(UnmanagedType.LPStr)] string format, [MarshalAs(UnmanagedType.LPStr)] string message);
#endif
}