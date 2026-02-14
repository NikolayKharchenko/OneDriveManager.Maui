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
        try
        {
            lock (Gate)
            {
                CloseWriterNoThrow();

                var baseDir = GetBaseDir();
                Directory.CreateDirectory(baseDir);
                File.WriteAllText(Path.Combine(baseDir, LogFileName), string.Empty, Encoding.UTF8);

                _sessionStarted = false;
            }

            WriteCore("StartupLog:Clear ok");
        }
        catch (Exception ex)
        {
            WriteCore($"StartupLog:Clear failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    public static void Write(string message) => WriteCore(message);

    public static void Write(Exception ex, string message = "Exception")
        => WriteCore($"{message}: {ex}");

    private static void EnsureWriter()
    {
        if (_writer != null)
            return;

        var baseDir = GetBaseDir();
        Directory.CreateDirectory(baseDir);

        var path = Path.Combine(baseDir, LogFileName);

        // Keep the file open, append, allow container tools to read it.
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
            _writer!.Flush();
            _stream!.Flush(flushToDisk: true);
        }
        catch
        {
            // ignore
        }
    }

    private static void CloseWriterNoThrow()
    {
        try { _writer?.Dispose(); } catch { }
        try { _stream?.Dispose(); } catch { }
        _writer = null;
        _stream = null;
    }

    private static void WriteCore(string message)
    {
        var line = $"{DateTimeOffset.Now:O} {message}";

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