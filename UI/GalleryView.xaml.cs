namespace OneDriveAlbums.UI;

public partial class GalleryView : ContentView
{
	public GalleryView()
	{
		StartupLog.Write("GalleryView.ctor");
		InitializeComponent();
        StartupLog.Write("GalleryView.InitializeComponent finished");
    }

    public void ShowGallery(string url)
	{
		WebVw.IsVisible = false;
        WebVw.Source = url;
    }

    private void WebVw_Navigated(object? sender, WebNavigatedEventArgs e)
    {
        WebVw.IsVisible = true;
    }
}