using Microsoft.Graph.Models;
using System.Diagnostics;

namespace OneDriveManager.Graph;

public static class GraphExtensions
{
    static public bool Duplicates(this DriveItem first, DriveItem second)
    {
        Trace.Assert(first?.File?.Hashes?.QuickXorHash != null);
        Trace.Assert(second?.File?.Hashes?.QuickXorHash != null);
        return first.Size == second.Size && first.File.Hashes.QuickXorHash == second.File.Hashes.QuickXorHash;
    }

    static public IReadOnlyList<DriveItem> SortForDuplicateSearch(this IReadOnlyList<DriveItem> items)
    {
        return items!.OrderBy(item => item.Size)
            .ThenBy(item => item.File?.Hashes?.QuickXorHash)
            .ThenBy(item => item.CreatedDateTime)
            .ToList();
    }

}

