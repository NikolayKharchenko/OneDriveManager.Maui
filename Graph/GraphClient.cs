using Microsoft.Graph;
using Microsoft.Graph.Drives.Item.Bundles;
using Microsoft.Graph.Drives.Item.Items;
using Microsoft.Graph.Drives.Item.Items.Item;
using Microsoft.Graph.Drives.Item.Root;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace OneDriveAlbums.Graph;

public enum ThumbnailSize
{
    Small,
    Medium,
    Large
}

public class BundleMetadata
{
    public string Name { get; set; } = string.Empty;
    public DateTime? CoverImageTakenDate { get; set; }
    public DateTime MinDate { get; set; } = DateTime.MaxValue;
    public DateTime MaxDate { get; set; } = DateTime.MinValue;
}

public class GraphClient
{
    internal sealed class PersistentData
    {
        public Dictionary<string, BundleMetadata> Bundle { get; set; } = new();
    }

    public static readonly string[] Scopes =
    [
        "User.Read",
        "Files.ReadWrite",
        "offline_access",
    ];

    public record struct Config(
        int MaxElements = int.MaxValue
    );

    GraphServiceClient? client;
    Config config;
    static GraphClient? instance;

    const string persistentDataOneDrivePath = "/.OneDriveManager/persistdb.json";
    private PersistentData persistentStore { get; set; } = new();

    public GraphClient()
    {
        Trace.Assert(instance == null, "GraphClient is a singleton");
        instance = this;
    }

    public static GraphClient Instance
    {
        get
        {
            if (instance == null)
                instance = new GraphClient();
            return instance;
        }
    }

    public bool IsConnected => client != null;

    public record ItemsEventArgs(int Count, TimeSpan Elapsed);

    public event EventHandler<ItemsEventArgs>? Items_Loading;
    public event EventHandler<ItemsEventArgs>? Items_Loaded;

    public async Task Connect(Config config, IAuthenticationProvider authProvider)
    {
        try
        {
            this.config = config;

            var adapter = new HttpClientRequestAdapter(authProvider, httpClient: new HttpClient());
            client = new GraphServiceClient(adapter);

            await client.Me.GetAsync();

            await loadPersistentDataAsync();
        }
        catch
        {
            client = null;
            throw;
        }
    }

    private async Task loadPersistentDataAsync()
    {
        Trace.Assert(client != null, "GraphClient is not connected");

        DriveItem? persistentItem = await TryGetItemByPathAsync(persistentDataOneDrivePath);
        if (persistentItem?.Id is null)
        {
            persistentStore = new PersistentData();
            return;
        }

        string driveId = await getDriveIdAsync();

        Stream? content = await client.Drives[driveId].Items[persistentItem.Id].Content.GetAsync();
        if (content == null)
        {
            persistentStore = new PersistentData();
            return;
        }

        PersistentData? loaded = await JsonSerializer.DeserializeAsync(
            content,
            GraphJsonContext.Default.PersistentData);

        persistentStore = loaded ?? new PersistentData();
    }

    public async Task StorePersistentData()
    {
        Trace.Assert(client != null, "GraphClient is not connected");

        string driveId = await getDriveIdAsync();

        string json = JsonSerializer.Serialize(
            persistentStore,
            GraphJsonContext.Default.PersistentData);

        MemoryStream body = new(System.Text.Encoding.UTF8.GetBytes(json));

        DriveItem? persistentItem = await TryGetItemByPathAsync(persistentDataOneDrivePath);
        if (persistentItem?.Id is null)
        {
            await client.Drives[driveId].Root.ItemWithPath(persistentDataOneDrivePath).Content.PutAsync(body);
        }
        else
        {
            await client.Drives[driveId].Items[persistentItem.Id].Content.PutAsync(body);
        }
    }

    public async Task<string> ConnectedAccountName()
    {
        Trace.Assert(client != null, "GraphClient is not connected");
        User? me = await client.Me.GetAsync();
        return me?.DisplayName ?? "(unknown)";
    }

    private async Task<IReadOnlyList<DriveItem>> getItemsAsync(
        Func<string, Task<DriveItemCollectionResponse?>> urlToItems, string startUrl, Func<DriveItem, bool> filter)
    {
        Trace.Assert(client != null, "GraphClient is not connected");

        List<DriveItem> all = [];
        DateTime startTime = DateTime.Now;

        string? nextUrl = startUrl;
        Items_Loading?.Invoke(null, new ItemsEventArgs(all.Count, DateTime.Now - startTime));

        while (!string.IsNullOrEmpty(nextUrl) && all.Count < config.MaxElements)
        {
            DriveItemCollectionResponse? page = await urlToItems(nextUrl);

            if (page?.Value != null)
            {
                all.AddRange(page.Value.Where(filter));
                Items_Loading?.Invoke(null, new ItemsEventArgs(all.Count, DateTime.Now - startTime));
            }

            nextUrl = page?.OdataNextLink;
        }

        if (all.Count > config.MaxElements)
            all.RemoveRange(config.MaxElements, all.Count - config.MaxElements);
        Items_Loaded?.Invoke(null, new ItemsEventArgs(all.Count, DateTime.Now - startTime));
        return all;
    }

    public async Task<IReadOnlyList<DriveItem>> GetDescendantsAsync(Func<DriveItem, bool> filter)
    {
        Trace.Assert(client != null, "GraphClient is not connected");

        string startUrl = "https://graph.microsoft.com/v1.0/me/drive/root/delta";
        ItemsRequestBuilder itemsRequestBuilder = client.Drives["root"].Items;
        return await getItemsAsync(url => itemsRequestBuilder.WithUrl(url).GetAsync(), startUrl, filter);
    }

    public async Task<IReadOnlyList<DriveItem>> GetBundlesAsync(Func<DriveItem, bool> filter)
    {
        Trace.Assert(client != null, "GraphClient is not connected");

        string startUrl = "https://graph.microsoft.com/v1.0/drive/bundles";
        BundlesRequestBuilder bundlesRequestBuilder = client.Drives["root"].Bundles;
        IReadOnlyList<DriveItem> result = await getItemsAsync(url => bundlesRequestBuilder.WithUrl(url).GetAsync(), startUrl, filter);
        return result;
    }

    private string? _driveId;

    private async Task<string> getDriveIdAsync()
    {
        if (!string.IsNullOrWhiteSpace(_driveId))
            return _driveId;

        Trace.Assert(client != null, "GraphClient is not connected");
        Drive? drive = await client.Me.Drive.GetAsync();
        Trace.Assert(!string.IsNullOrWhiteSpace(drive?.Id), "Unable to resolve drive id.");
        return _driveId = drive!.Id!;
    }

    public async Task<IReadOnlyList<DriveItem>> GetBundleChildrenAsync(DriveItem bundle)
    {
        Trace.Assert(client != null, "GraphClient is not connected");
        Trace.Assert(!string.IsNullOrWhiteSpace(bundle.Id), "Bundle Id is missing");

        DriveItemCollectionResponse? children = await (await getItemRequestBuilderAsync(bundle.Id)).Children.GetAsync();
        return children?.Value ?? [];
    }

    public async Task DeleteBundleChildAsync(DriveItem bundle, string childId)
    {
        Trace.Assert(client != null, "GraphClient is not connected");
        Trace.Assert(!string.IsNullOrWhiteSpace(bundle.Id), "Bundle Id is missing");
        Trace.Assert(!string.IsNullOrWhiteSpace(childId), "Child Id is missing");

        string url = $"https://graph.microsoft.com/v1.0/drive/bundles/{bundle.Id}/children/{childId}";

        var requestInfo = new RequestInformation
        {
            HttpMethod = Method.DELETE,
            UrlTemplate = url
        };

        await client.RequestAdapter.SendNoContentAsync(requestInfo);
    }

    private async Task<DriveItemItemRequestBuilder> getItemRequestBuilderAsync(string itemId)
    {
        Trace.Assert(client != null, "GraphClient is not connected");
        string driveId = await getDriveIdAsync();
        Trace.Assert(!string.IsNullOrWhiteSpace(driveId), "Unable to resolve drive id.");
        return client.Drives[driveId].Items[itemId];
    }

    public async Task<string?> GetThumbnailUrlAsync(string itemId, ThumbnailSize preferredSize)
    {
        DriveItemItemRequestBuilder builder = await getItemRequestBuilderAsync(itemId);
        ThumbnailSetCollectionResponse? response = await builder.Thumbnails.GetAsync();
        ThumbnailSet? set = response?.Value?.FirstOrDefault();
        string? preferredUrl = preferredSize switch
        {
            ThumbnailSize.Small => set?.Small?.Url,
            ThumbnailSize.Medium => set?.Medium?.Url,
            ThumbnailSize.Large => set?.Large?.Url,
            _ => null,
        };
        return preferredUrl ?? set?.Medium?.Url ?? set?.Large?.Url ?? set?.Small?.Url;
    }

    public async Task DeleteItemAsync(string itemId)
    {
        DriveItemItemRequestBuilder builder = await getItemRequestBuilderAsync(itemId);
        await builder.DeleteAsync();
    }

    static DateTime photoDate(DriveItem item) => item.Photo?.TakenDateTime?.DateTime ?? item.CreatedDateTime!.Value.DateTime;

    static DateTime dateMax(DateTime a, DateTime b) => a > b ? a : b;
    static DateTime dateMin(DateTime a, DateTime b) => a < b ? a : b;

    async Task<DriveItem?> getCoverImageItem(DriveItem bundle)
    {
        string? coverImageId = bundle?.Bundle?.Album?.CoverImageItemId;
        if (coverImageId == null)
            return null;
        DriveItemItemRequestBuilder builder = await getItemRequestBuilderAsync(coverImageId);
        return await builder.GetAsync();
    }

    public async Task CollectBundleMetadata(DriveItem bundle)
    {
        IReadOnlyList<DriveItem> children = await GetBundleChildrenAsync(bundle);
        BundleMetadata metadata = new() { Name = bundle.Name! };
        foreach (DriveItem child in children)
        {
            DateTime childDate = photoDate(child);
            metadata.MinDate = dateMin(metadata.MinDate, childDate);
            metadata.MaxDate = dateMax(metadata.MaxDate, childDate);
        }
        DriveItem? coverImageItem = await getCoverImageItem(bundle);
        metadata.CoverImageTakenDate = coverImageItem?.Photo?.TakenDateTime?.DateTime ?? metadata.MinDate;
        persistentStore.Bundle[bundle.Id!] = metadata;
    }

    public BundleMetadata? GetBundleMetadata(string bundleId)
    {
        if (persistentStore.Bundle.TryGetValue(bundleId, out BundleMetadata? metadata))
            return metadata;
        return null;
    }

    public async Task<DriveItem?> TryGetItemByPathAsync(string oneDrivePathFromRoot)
    {
        Trace.Assert(client != null, "GraphClient is not connected");

        string path = NormalizeOneDrivePath(oneDrivePathFromRoot);

        string driveId = await getDriveIdAsync();
        RootRequestBuilder root = client.Drives[driveId].Root;

        if (string.IsNullOrEmpty(path))
            return await root.GetAsync();

        try
        {
            // GET /drives/{driveId}/root:/path:
            return await root.ItemWithPath(path).GetAsync();
        }
        catch (ApiException ex) when (ex.ResponseStatusCode == (int)HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<DriveItem> GetItemByPathAsync(string oneDrivePathFromRoot)
    {
        DriveItem? item = await TryGetItemByPathAsync(oneDrivePathFromRoot);
        return item ?? throw new FileNotFoundException($"OneDrive item not found: '{oneDrivePathFromRoot}'");
    }

    private static string NormalizeOneDrivePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        path = path.Replace('\\', '/');
        if (path.StartsWith('/'))
            path = path[1..];

        return path;
    }
}