using System.Text.Json;
using System.Text.Json.Serialization;

namespace OneDriveAlbums.Graph;

[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true,
    WriteIndented = true)]
[JsonSerializable(typeof(GraphClient.PersistentData))]
[JsonSerializable(typeof(BundleMetadata))]
[JsonSerializable(typeof(Dictionary<string, BundleMetadata>))]
internal sealed partial class GraphJsonContext : JsonSerializerContext
{
}