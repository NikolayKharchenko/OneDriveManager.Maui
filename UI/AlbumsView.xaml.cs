using Microsoft.Graph.Models;
using OneDriveAlbums.Graph;
using System.Collections.ObjectModel;

namespace OneDriveAlbums.UI;

public partial class AlbumsView : ContentView
{
    private IReadOnlyList<DriveItem>? albums;
    private readonly ObservableCollection<AlbumItemModel> albumModels = new();

    bool? ascByName;
    bool? ascByDate;

    public AlbumsView()
    {
        InitializeComponent();
        Albums_CVw.ItemsSource = albumModels;
    }

    static bool isItemSuitable(DriveItem item) => item?.Bundle?.Album != null;

    public async Task LoadAlbums()
    {
        albums = await GraphClient.Instance.GetBundlesAsync(isItemSuitable);

        albumModels.Clear();
        foreach (DriveItem album in albums)
            albumModels.Add(new AlbumItemModel(album));

        loadAllThumbnails();
    }

    private void loadAllThumbnails()
    {
        foreach (AlbumItemModel model in albumModels)
            _ = model.LoadThumbnailAsync();
    }

    public void Album_Tap(object sender, EventArgs e)
    {
        if (sender is not Border border || border.BindingContext is not AlbumItemModel albumModel)
            return;

        if (albumModel.Item.WebUrl is null)
            return;

        _ = Launcher.OpenAsync(albumModel.Item.WebUrl);
    }

    public void sortAlbums<T>(Func<AlbumItemModel, T> selector, bool ascending)
    {
        // important: materialize before Clear()
        List<AlbumItemModel> sorted = (ascending ? albumModels.OrderBy(selector) : albumModels.OrderByDescending(selector)).ToList();
        
        albumModels.Clear();

        foreach (AlbumItemModel model in sorted)
            albumModels.Add(model);

        // Old thumbnails can expire. Need to reload them
        loadAllThumbnails();
    }

    public void SortByName_Click(object sender, EventArgs e)
    {
        ascByDate = null;
        ascByName = ascByName is null ? true : !ascByName;
        sortAlbums(model => model.Name, ascByName.Value);
    }

    public void SortByDate_Click(object sender, EventArgs e)
    {
        ascByName = null;
        ascByDate = ascByDate is null ? false : !ascByDate;

        sortAlbums(model => model.Item!.CreatedDateTime!.Value, ascByDate.Value);
    }
}