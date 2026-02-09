using System.Diagnostics;

#if IOS
using Foundation;
#endif

namespace OneDriveAlbums.UI;

internal static class StartupLog
{
    private const string LogFileName = "startup.log";
    private static readonly object Gate = new();

    private static string GetBaseDir()
    {
        // FileSystem.* can fail very early in startup; this is safer.
        try
        {
            var dir = FileSystem.AppDataDirectory;
            if (!string.IsNullOrWhiteSpace(dir))
                return dir;
        }
        catch
        {
            // ignore
        }

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
        var line = $"{DateTimeOffset.UtcNow:O} {message}";

        // Always emit to debug output
        try { Trace.WriteLine(line); } catch { /* ignore */ }

#if IOS
        // Also emit to unified logging so Console.app can see it
        try { NSLog($"{line}"); } catch { /* ignore */ }
#endif

        // Best-effort file append (may fail early startup)
        try
        {
            lock (Gate)
            {
                var baseDir = GetBaseDir();
                Directory.CreateDirectory(baseDir);
                File.AppendAllText(Path.Combine(baseDir, LogFileName), line + Environment.NewLine);
            }
        }
        catch
        {
            // ignore
        }
    }
}