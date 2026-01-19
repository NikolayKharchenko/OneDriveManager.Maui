using Microsoft.Graph.Models;
using OneDriveAlbums.Graph;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OneDriveAlbums.UI;

public sealed class AlbumItemModel : INotifyPropertyChanged
{
    public AlbumItemModel(DriveItem item) => Item = item;

    public DriveItem Item { get; }
    public string Name => Item.Name ?? "(unnamed)";

    ImageSource? thumbnail;
    public ImageSource? Thumbnail
    {
        get => thumbnail;
        private set
        {
            if (ReferenceEquals(thumbnail, value))
                return;
            thumbnail = value;
            OnPropertyChanged();
        }
    }

    bool thumbnailRequested;

    public async Task EnsureThumbnailAsync(ThumbnailSize preferredSize = ThumbnailSize.Large)
    {
        if (thumbnailRequested)
            return;

        thumbnailRequested = true;

        if (string.IsNullOrWhiteSpace(Item.Id))
            return;

        string? coverImageItemId = Item.Bundle?.Album?.CoverImageItemId;
        if (string.IsNullOrWhiteSpace(coverImageItemId))
            return;
        string? url = await GraphClient.Instance.GetThumbnailUrlAsync(coverImageItemId, preferredSize).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(url))
            return;

        MainThread.BeginInvokeOnMainThread(() => Thumbnail = ImageSource.FromUri(new Uri(url)));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}