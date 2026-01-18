using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace OneDriveAlbums.UI;

internal static class OneDriveFolderLocator
{
    // FOLDERID_OneDrive
    private static readonly Guid FolderIdOneDrive = new("A52BBA46-E9E1-435F-B3D9-28DAA648C0F6");

    public static string? TryGetConsumerOneDriveRoot()
    {
        if (!OperatingSystem.IsWindows())
            return null;

        string? knownFolder = TryGetKnownFolderPath(FolderIdOneDrive);
        if (!string.IsNullOrWhiteSpace(knownFolder) && Directory.Exists(knownFolder))
            return knownFolder;

        const string oneDriveKey = @"HKEY_CURRENT_USER\Software\Microsoft\OneDrive";
        string? fromRegistry = Registry.GetValue(oneDriveKey, "UserFolder", null) as string;
        if (!string.IsNullOrWhiteSpace(fromRegistry) && Directory.Exists(fromRegistry))
            return fromRegistry;

        return null;
    }

    private static string? TryGetKnownFolderPath(Guid folderId)
    {
        int hr = SHGetKnownFolderPath(folderId, 0, IntPtr.Zero, out IntPtr ppszPath);
        if (hr != 0 || ppszPath == IntPtr.Zero)
            return null;

        try
        {
            return Marshal.PtrToStringUni(ppszPath);
        }
        finally
        {
            Marshal.FreeCoTaskMem(ppszPath);
        }
    }

    [DllImport("shell32.dll")]
    private static extern int SHGetKnownFolderPath(
        [MarshalAs(UnmanagedType.LPStruct)] Guid rfid,
        uint dwFlags,
        IntPtr hToken,
        out IntPtr ppszPath);
}