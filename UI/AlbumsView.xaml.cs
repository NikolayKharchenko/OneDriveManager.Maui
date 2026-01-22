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

    bool? ascByName;
    bool? ascByDate;

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

    public async Task LoadAlbums()
    {
        albums = await GraphClient.Instance.GetBundlesAsync(isItemSuitable);
        reloadModels();
        SortByDate_Click(this, EventArgs.Empty);
    }

    private void reloadModels()
    {
        albumModels.Clear();
        if (albums is null)
            return;

        foreach (DriveItem album in albums)
            albumModels.Add(new AlbumItemModel(album));
    }

    private async void ThumbnailImage_BindingContextChanged(object sender, EventArgs e)
    {
        if (sender is not Image img || img.BindingContext is not AlbumItemModel model)
            return;

        await model.LoadThumbnailAsync();
    }

    public void Album_Tap(object sender, EventArgs e)
    {
        if (sender is not Border border || border.BindingContext is not AlbumItemModel albumModel)
            return;

        if (albumModel.Item.WebUrl is null)
            return;

        _ = Launcher.OpenAsync(albumModel.Item.WebUrl);
    }

    void sortAlbums<T>(Func<AlbumItemModel, T> selector, bool ascending)
    {
        List<AlbumItemModel> sorted = (ascending ? albumModels.OrderBy(selector) : albumModels.OrderByDescending(selector)).ToList();

        albumModels.Clear();

        foreach (AlbumItemModel model in sorted)
            albumModels.Add(model);
    }

    void updateSortButtons()
    {
        SortByName_Icon.Text = sortIcon(ascByName);
        SortByDate_Icon.Text = sortIcon(ascByDate);
    }

    public void SortByName_Click(object sender, EventArgs e)
    {
        ascByDate = null;
        ascByName = ascByName is null ? true : !ascByName;
        sortByCurrentState();
    }

    public void SortByDate_Click(object sender, EventArgs e)
    {
        ascByName = null;
        ascByDate = ascByDate is null ? false : !ascByDate;
        sortByCurrentState();
    }

    void sortByCurrentState()
    {
        if (ascByName is not null)
            sortAlbums(model => model.Name, ascByName.Value);
        else if (ascByDate is not null)
            sortAlbums(model => model.Item!.CreatedDateTime!.Value, ascByDate.Value);

        updateSortButtons();
    }

    void SearchFor_TextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = e.NewTextValue?.Trim() ?? string.Empty;
        if (searchText.Length == 0)
        {
            reloadModels();
            ascByDate = ascByName = null;
            updateSortButtons();
            return;
        }
        if (searchText.Length < 3)
            return;

        albumModels.Clear();
        IEnumerable<AlbumItemModel> filtered = albums!
                .Where(album => (album.Name?.Contains(searchText, StringComparison.OrdinalIgnoreCase)).GetValueOrDefault(false))
                .Select(album => new AlbumItemModel(album));

        foreach (AlbumItemModel model in filtered)
            albumModels.Add(model);
    }

    void ClearSearch_Click(object sender, EventArgs e)
    {
        SearchFor_Entry.Text = string.Empty;
    }

    private void Albums_SizeChanged(object sender, EventArgs e)
    {
        GridItemsLayout grid = (GridItemsLayout)Albums_CVw.ItemsLayout;
        grid.Span = int.Max(1, (int)(Albums_CVw.Width / 300));
    }
}