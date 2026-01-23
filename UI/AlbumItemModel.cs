using Microsoft.Graph.Models;
using OneDriveAlbums.Graph;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OneDriveAlbums.UI;

public sealed class AlbumItemModel : INotifyPropertyChanged
{
    public AlbumItemModel(DriveItem item, BundleMetadata? metadata)
    {
        Item = item;
        bundleMetadata = metadata;
    }

    private BundleMetadata? bundleMetadata;
    public DriveItem Item { get; }

    public DateTime DateForSort => bundleMetadata?.CoverImageTakenDate ?? Item.CreatedDateTime!.Value.DateTime;
    public string Name => Item.Name ?? "(unnamed)";
    public string DatesRange 
    { 
        get
        {
            if (bundleMetadata == null || bundleMetadata!.MinDate == DateTime.MinValue || bundleMetadata.MaxDate == DateTime.MaxValue)
                return DateForSort.ToString("MMM yyyy");
            if (bundleMetadata.MinDate.Year == bundleMetadata.MaxDate.Year && bundleMetadata.MinDate.Month == bundleMetadata.MaxDate.Month)
                return bundleMetadata.MinDate.ToString("MMM yyyy");
            return $"{bundleMetadata.MinDate.ToString("MMM yyyy")} - {bundleMetadata.MaxDate.ToString("MMM yyyy")}";
        }
    }

    private ImageSource? thumbnail;
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

    public async Task LoadThumbnailAsync(ThumbnailSize preferredSize = ThumbnailSize.Large)
    {
        if (string.IsNullOrWhiteSpace(Item.Id))
            return;

        string? coverImageItemId = Item.Bundle?.Album?.CoverImageItemId;
        if (string.IsNullOrWhiteSpace(coverImageItemId))
            return;

        string? url = await GraphClient.Instance.GetThumbnailUrlAsync(coverImageItemId, preferredSize);
        if (string.IsNullOrWhiteSpace(url))
            return;

        MainThread.BeginInvokeOnMainThread(() => Thumbnail = ImageSource.FromUri(new Uri(url)));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
