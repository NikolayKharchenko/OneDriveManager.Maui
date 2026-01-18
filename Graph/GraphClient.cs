using Microsoft.Graph;
using Microsoft.Graph.Drives.Item.Bundles;
using Microsoft.Graph.Drives.Item.Items;
using Microsoft.Graph.Drives.Item.Items.Item;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Abstractions;
using System.Diagnostics;
using System.Text.Json;

namespace OneDriveAlbums.Graph;

public enum ThumbnailSize
{
    Small,
    Medium,
    Large
}

public class GraphClient
{
    internal record PersistentData(Dictionary<string, DateTime> BundleDates);

    private static readonly string[] Scopes =
    [
        "User.Read",
        "Files.ReadWrite",
        "offline_access",
    ];

    public record struct Config(
        string OneDriveRootFolder,
        int MaxElements = int.MaxValue
    );

    GraphServiceClient? client;
    Config config;
    static GraphClient? instance;

    private string configDir => Path.Combine(config.OneDriveRootFolder, ".OneDriveManager");
    string persistentDataPath => Path.Combine(configDir, "persistdb.json");
    private PersistentData persistentStore { get; set; } = new(new());
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        WriteIndented = true,
    };


    public GraphClient()
    {
        Trace.Assert(instance == null, "GraphClient is a singleton");
        instance = this;
    }

    public static GraphClient Instance()
    {
        if (instance == null)
        {
            instance = new GraphClient();
        }
        return instance;
    }

    public bool IsConnected => client != null;

    public record ItemsEventArgs(int Count, TimeSpan Elapsed);

    public event EventHandler<ItemsEventArgs>? Items_Loading;
    public event EventHandler<ItemsEventArgs>? Items_Loaded;
    public event EventHandler? Connection_Change;

    public async Task Connect(Config config, IAuthProvider authProvider)
    {
        try
        {
            this.config = config;

            var kiotaAuthProvider = new DelegatingKiotaAuthProvider(authProvider, Scopes);
            var adapter = new HttpClientRequestAdapter(kiotaAuthProvider, httpClient: new HttpClient());

            client = new GraphServiceClient(adapter);
            await client.Me.GetAsync();

            loadPersistentData();
            Connection_Change?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception)
        {
            client = null;
            Connection_Change?.Invoke(this, EventArgs.Empty);
            throw;
        }
    }

    private void loadPersistentData()
    {
        if (!File.Exists(persistentDataPath))
            return;
        string json = File.ReadAllText(persistentDataPath);
        persistentStore = JsonSerializer.Deserialize<PersistentData>(json, JsonOptions)!;
    }

    private void storePersistentData()
    {
        Directory.CreateDirectory(configDir);
        string json = JsonSerializer.Serialize(persistentStore, JsonOptions);
        File.WriteAllText(persistentDataPath, json);
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

        // Delta enumerates the whole drive state efficiently, then provides a deltaLink for later incremental sync.
        // Initial request:
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
        foreach (DriveItem item in result)
        {
            if (persistentStore.BundleDates.TryGetValue(item.Id!, out DateTime storedDate))
                item.CreatedDateTime = storedDate;
        }
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

    public async Task<bool> TryFixCreationDateAsync(DriveItem item, string childItemId)
    {
        DriveItemItemRequestBuilder childBuilder = await getItemRequestBuilderAsync(childItemId);
        DriveItem child = (await childBuilder.GetAsync())!;
        bool hasTakenDate = child.Photo?.TakenDateTime != null;
        if (hasTakenDate)
            item.CreatedDateTime = persistentStore.BundleDates[item.Id!] = child.Photo!.TakenDateTime!.Value.DateTime;
        storePersistentData();
        return hasTakenDate;
    }
}