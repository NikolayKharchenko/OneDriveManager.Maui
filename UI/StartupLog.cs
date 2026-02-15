using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace OneDriveAlbums.UI;

internal static class StartupLog
{
    private const string LogFileName = "startup.log";
    private static readonly object Gate = new();

    private static FileStream? _stream;
    private static StreamWriter? _writer;
    private static bool _sessionStarted;

    private static string GetBaseDir()
    {
#if IOS
        try
        {
            // iOS Documents directory (visible via Finder/Files when UIFileSharingEnabled=true)
            // In Xamarin/.NET for iOS, SpecialFolder.MyDocuments can map to ".../Documents".
            var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (!string.IsNullOrWhiteSpace(docs))
                return docs;

            // Fallback: ask Foundation directly to avoid any unexpected mapping.
            var urls = Foundation.NSFileManager.DefaultManager.GetUrls(Foundation.NSSearchPathDirectory.DocumentDirectory, Foundation.NSSearchPathDomain.User);
            var url = urls?.FirstOrDefault();
            var docPath = url?.Path;
            if (!string.IsNullOrWhiteSpace(docPath))
                return docPath;
        }
        catch { }
#endif

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

    private static void EnsureWriter()
    {
        if (_writer != null)
            return;

        var baseDir = GetBaseDir();
        Directory.CreateDirectory(baseDir);

        // Force-create a visible marker file so Finder shows something even on early crash.
        // If you don't see this file, you're looking at the wrong container or baseDir isn't Documents.
        var markerPath = Path.Combine(baseDir, "log_marker.txt");
        try
        {
            File.WriteAllText(markerPath, $"created {DateTimeOffset.UtcNow:O}", Encoding.UTF8);
        }
        catch { }

        var path = LogPath;

        _stream = new FileStream(
            path,
            FileMode.Append,
            FileAccess.Write,
            FileShare.ReadWrite,
            bufferSize: 4096,
            FileOptions.WriteThrough);

        _writer = new StreamWriter(_stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
        {
            AutoFlush = false
        };
    }

    private static void StartSession_NoThrow()
    {
        if (_sessionStarted)
            return;

        _sessionStarted = true;

        try
        {
            _writer!.WriteLine($"{DateTimeOffset.UtcNow:O} [StartupLog] ===== session start =====");
            _writer!.WriteLine($"{DateTimeOffset.UtcNow:O} [StartupLog] LogPath={LogPath}");
            _writer!.Flush();
            _stream!.Flush(flushToDisk: true);
        }
        catch
        {
            // ignore
        }
    }

    public static void Write(string message) => WriteCore(message);

    public static void Write(Exception ex, string message = "Exception")
        => WriteCore($"{message}: {ex}");

    private static void WriteCore(string message)
    {
        var line = $"{DateTimeOffset.Now:O} [StartupLog] {message}";

        try { Trace.WriteLine(line); } catch { }

#if IOS
        try { IOSLog(line); } catch { }
#endif

        try
        {
            lock (Gate)
            {
                EnsureWriter();
                StartSession_NoThrow();

                _writer!.WriteLine(line);
                _writer!.Flush();
                _stream!.Flush(flushToDisk: true);
            }
        }
        catch
        {
            // ignore
        }
    }

#if IOS
    private static void IOSLog(string message)
        => os_log("%{public}s", message);

    [DllImport("/usr/lib/libSystem.dylib", EntryPoint = "os_log")]
    private static extern void os_log([MarshalAs(UnmanagedType.LPStr)] string format, [MarshalAs(UnmanagedType.LPStr)] string message);
#endif
}