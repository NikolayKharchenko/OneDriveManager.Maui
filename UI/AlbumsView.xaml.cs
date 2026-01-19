using Microsoft.Graph.Models;
using OneDriveAlbums.Graph;
using System.Collections.ObjectModel;

namespace OneDriveAlbums.UI;

public partial class AlbumsView : ContentView
{
    private IReadOnlyList<DriveItem>? albums;
    private readonly ObservableCollection<AlbumItemModel> albumModels = new();

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

        foreach (var model in albumModels)
            _ = model.EnsureThumbnailAsync(); // fire-and-forget; VM marshals back to UI thread
    }

    public void OnAlbumTapped(object sender, EventArgs e)
    {
        if (sender is not Border border || border.BindingContext is not AlbumItemModel albumModel)
            return;

        if (albumModel.Item.WebUrl is null)
            return;

        _ = Launcher.OpenAsync(albumModel.Item.WebUrl);
    }
}