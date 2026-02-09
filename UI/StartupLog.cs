using System.Diagnostics;

namespace OneDriveAlbums.UI;

internal static class StartupLog
{
    private const string LogFileName = "startup.log";
    private static readonly object Gate = new();

    public static string LogPath =>
        Path.Combine(FileSystem.AppDataDirectory, LogFileName);

    public static void Clear()
    {
        try
        {
            lock (Gate)
            {
                Directory.CreateDirectory(FileSystem.AppDataDirectory);
                File.WriteAllText(LogPath, string.Empty);
            }
        }
        catch
        {
            // ignore
        }
    }

    public static void Write(string message)
    {
        try
        {
            var line = $"{DateTimeOffset.UtcNow:O} {message}{Environment.NewLine}";
            lock (Gate)
            {
                Directory.CreateDirectory(FileSystem.AppDataDirectory);
                File.AppendAllText(LogPath, line);
            }

            // Also emit to device logs when available
            Trace.WriteLine(line);
        }
        catch
        {
            // ignore
        }
    }

    public static void Write(Exception ex, string message = "Exception")
        => Write($"{message}: {ex}");
}