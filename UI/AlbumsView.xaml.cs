using Microsoft.Graph.Models;
using OneDriveAlbums.Graph;
using OneDriveAlbums.UI.Resources.Strings;
using System.Collections.ObjectModel;

using Image = Microsoft.Maui.Controls.Image;
namespace OneDriveAlbums.UI;

public partial class AlbumsView : ContentView
{
    private IReadOnlyList<DriveItem>? albums;
    private readonly ObservableCollection<AlbumItemModel> albumModels = new();

    bool? nameAscending;
    bool? dateAscending;

    const string UpIcon = "arrow_drop_up";
    const string DownIcon = "arrow_drop_down";

    string sortIcon(bool? ascending)
    {
        if (ascending is null)
            return string.Empty;

        return ascending.Value ? UpIcon : DownIcon;
    }

    public AlbumsView()
    {
        InitializeComponent();
        Albums_CVw.ItemsSource = albumModels;
    }

    static bool isItemSuitable(DriveItem item) => item?.Bundle?.Album != null;
    private static AlbumItemModel createModel(DriveItem item) => new(item, GraphClient.Instance.GetBundleMetadata(item.Id!));

    public async Task LoadAlbums()
    {
        albums = await GraphClient.Instance.GetBundlesAsync(isItemSuitable);
        reloadModels();
        sortBy(nameAsc: null, dateAsc: false);
    }

    private void reloadModels()
    {
        albumModels.Clear();
        if (albums is null)
            return;

        foreach (DriveItem album in albums)
            albumModels.Add(createModel(album));
    }

    private async void ThumbnailImage_BindingContextChanged(object? sender, EventArgs e)
    {
        if (sender is not Image img || img.BindingContext is not AlbumItemModel model)
            return;

        await model.LoadThumbnailAsync();
    }

    private async Task openAlbum(object? sender)
    {
        if (sender is not Border border || border.BindingContext is not AlbumItemModel albumModel)
            return;
        if (albumModel.Item.WebUrl is null)
            return;

#if ANDROID
        _ = Browser.OpenAsync(albumModel.Item.WebUrl, BrowserLaunchMode.SystemPreferred);
#else
        _ = Browser.OpenAsync(albumModel.Item.WebUrl, BrowserLaunchMode.External);
#endif
    }

    public void Album_Tap(object? sender, EventArgs e)
    {
#if !WINDOWS
        _ = openAlbum(sender);
#endif
    }

    public void Album_DblTap(object? sender, EventArgs e)
    {
#if WINDOWS
        _ = openAlbum(sender);
#endif
    }

    void sortAlbums<T>(Func<AlbumItemModel, T> selector, bool ascending)
    {
        List<AlbumItemModel> sorted = (ascending ? albumModels.OrderBy(selector) : albumModels.OrderByDescending(selector)).ToList();

        albumModels.Clear();
        sorted.ForEach(album => albumModels.Add(album));
    }

    public void SortByName_Click(object? sender, EventArgs e)
    {
        sortBy(nameAsc: nameAscending is null ? true : !nameAscending, dateAsc: null);
    }

    public void SortByDate_Click(object? sender, EventArgs e)
    {
        sortBy(nameAsc: null, dateAsc: dateAscending is null ? false : !dateAscending);
    }

    void sortBy(bool? nameAsc, bool? dateAsc)
    {
        nameAscending = nameAsc;
        dateAscending = dateAsc;
        if (nameAsc is not null)
            sortAlbums(model => model.Name, nameAsc.Value);
        else if (dateAsc is not null)
            sortAlbums(model => model.DateForSort, dateAsc.Value);

        SortByName_Icon.Ligature = sortIcon(nameAscending);
        SortByDate_Icon.Ligature = sortIcon(dateAscending);
    }

    void SearchFor_TextChanged(object? sender, TextChangedEventArgs e)
    {
        string searchText = e.NewTextValue?.Trim() ?? string.Empty;
        if (searchText.Length == 0)
        {
            reloadModels();
            sortBy(nameAscending, dateAscending);
            return;
        }
        if (searchText.Length < 3)
            return;

        albumModels.Clear();
        IEnumerable<AlbumItemModel> filtered = albums!
                .Where(album => (album.Name?.Contains(searchText, StringComparison.OrdinalIgnoreCase)).GetValueOrDefault(false))
                .Select(album => createModel(album));

        foreach (AlbumItemModel model in filtered)
            albumModels.Add(model);
    }

    void ClearSearch_Click(object? sender, EventArgs e)
    {
        SearchFor_Entry.Text = string.Empty;
    }

    private void Albums_SizeChanged(object? sender, EventArgs e)
    {
        GridItemsLayout grid = (GridItemsLayout)Albums_CVw.ItemsLayout;
        grid.Span = int.Max(1, (int)(Albums_CVw.Width / 300));
    }

    public async Task FixAllAlbumsMetadataAsync()
    {
        if (albums is null)
            return;

        int counter = 1;
        foreach (DriveItem album in albums)
        {
            MainPage.Instance!.SetStatusText(Strings.ProcessingItem_Msg, album.Name!, counter, albums.Count);
            await GraphClient.Instance.CollectBundleMetadata(album);
            counter++;
        }
        MainPage.Instance!.SetStatusText(Strings.Ready_Txt);

        await GraphClient.Instance.StorePersistentData();
        await LoadAlbums();
    }
}