using Microsoft.Graph.Models;
using OneDriveAlbums.Graph;
using OneDriveAlbums.UI.Resources.Strings;
using System.Diagnostics;

namespace OneDriveAlbums.UI;

public partial class MainPage : ContentPage
{
    private const string ClientId = "14667aef-1076-4332-a2aa-de72d31a570f";

    static public MainPage? Instance;
    public MainPage()
    {
        Trace.Assert(Instance == null);
        Instance = this;
		StartupLog.Write("MainPage.ctor");
        InitializeComponent();
        StartupLog.Write("MainPage.InitializeComponent passed");
        SetStatusText(Strings.Ready_Txt);
        GraphClient.Instance.Items_Loading += (s, e) => { SetStatusText(Strings.LoadingItems_Msg, e.Count, e.Elapsed); };
        GraphClient.Instance.Items_Loaded += (s, e) => { SetStatusText(Strings.LoadedItems_Msg, e.Count, e.Elapsed); };
        Dispatcher.Dispatch(initialize);
        StartupLog.Write("MainPage.ctor passed");
    }

    public void SetStatusText(string text = "", params object[] args)
    {
        Status_Lbl.Text = string.Format(text, args);
    }

    private async void initialize()
    {
        StartupLog.Write("MainPage.initialize called");
        await connectToGraph();
        StartupLog.Write("MainPage.connectToGraph finished");
        await Albums_Vw.LoadAlbums();
        StartupLog.Write("MainPage: albums loaded");
    }

    private async Task connectToGraph()
    {
        SetStatusText(Strings.Connecting_Msg);

        var auth = new MsalAuthProvider(ClientId);

        GraphClient.Config config = new(MaxElements: App.Config.MaxElements);
        await GraphClient.Instance.Connect(config, auth);

        string displayName = await GraphClient.Instance.ConnectedAccountName();
        SetStatusText(Strings.Connected_Msg, displayName);
    }

    public void ShowGalleryView(string url)
    {
        Gallery_Vw.ShowGallery(url);
        activateView(Gallery_Vw);
    }

    private void activateView(View view)
    {
        Settings_Vw.IsVisible = false;
        Albums_Vw.IsVisible = false;
        Gallery_Vw.IsVisible = false;
        view.IsVisible = true;
    }

    private void Settings_Click(object? sender, EventArgs e)
    {
        activateView(Settings_Vw);
    }

    private void Albums_Click(object? sender, EventArgs e)
    {
        activateView(Albums_Vw);
    }

    private async void Reconnect_Click(object? sender, EventArgs e)
    {
        Dispatcher.Dispatch(async () =>
        {
            // Clear MSAL token cache before reconnecting
            MsalAuthProvider auth = new(ClientId);
            await auth.ClearTokenCacheAsync();

            await connectToGraph();
            await Albums_Vw.LoadAlbums();
        });
    }
}
