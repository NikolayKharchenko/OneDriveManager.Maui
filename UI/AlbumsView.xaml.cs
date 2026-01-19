using Microsoft.Graph.Models;
using OneDriveAlbums.Graph;
using OneDriveAlbums.UI.Resources.Strings;
using System.Collections.ObjectModel;

namespace OneDriveAlbums.UI;

public partial class AlbumsView : ContentView
{
    private IReadOnlyList<DriveItem>? albums;
    private readonly ObservableCollection<AlbumItemModel> albumModels = new();

    bool? ascByName;
    bool? ascByDate;

    const char UpArrow = '\x2191';
    const char DownArrow = '\x2193';

    char sortSymbol(bool? ascending)
    {
        if (ascending is null)
            return ' ';
        return ascending.Value ? UpArrow : DownArrow;
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
    }

    private void reloadModels()
    {
        albumModels.Clear();
        if (albums is null)
            return;
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

    void sortAlbums<T>(Func<AlbumItemModel, T> selector, bool ascending)
    {
        // important: materialize before Clear()
        List<AlbumItemModel> sorted = (ascending ? albumModels.OrderBy(selector) : albumModels.OrderByDescending(selector)).ToList();
        
        albumModels.Clear();

        foreach (AlbumItemModel model in sorted)
            albumModels.Add(model);

        // Old thumbnails can expire. Need to reload them
        loadAllThumbnails();
    }

    void updateSortButtons()
    {
        SortByName_Btn.Text = Strings.SortByName_Btn + " " + sortSymbol(ascByName);
        SortByDate_Btn.Text = Strings.SortByDate_Btn + " " + sortSymbol(ascByDate);
    }

    public void SortByName_Click(object sender, EventArgs e)
    {
        ascByDate = null;
        ascByName = ascByName is null ? true : !ascByName;
        sortAlbums(model => model.Name, ascByName.Value);
        updateSortButtons();
    }

    public void SortByDate_Click(object sender, EventArgs e)
    {
        ascByName = null;
        ascByDate = ascByDate is null ? false : !ascByDate;

        sortAlbums(model => model.Item!.CreatedDateTime!.Value, ascByDate.Value);
        updateSortButtons();
    }

    void SearchFor_TextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = e.NewTextValue?.Trim() ?? string.Empty;
        if (searchText.Length == 0)
        {
            reloadModels();
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
        loadAllThumbnails();
    }

    void ClearSearch_Click(object sender, EventArgs e)
    {
        SearchFor_Entry.Text = string.Empty;
    }
}