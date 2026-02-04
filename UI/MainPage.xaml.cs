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
        Trace.Assert(Instance ==  null);
        Instance = this;
        InitializeComponent();
        SetStatusText(Strings.Ready_Txt);
        GraphClient.Instance.Items_Loading += (s, e) => { SetStatusText(Strings.LoadingItems_Msg, e.Count, e.Elapsed); };
        GraphClient.Instance.Items_Loaded += (s, e) => { SetStatusText(Strings.LoadedItems_Msg, e.Count, e.Elapsed); };
        Dispatcher.Dispatch(initialize);
    }

    public void SetStatusText(string text = "", params object[] args)
    {
        Status_Lbl.Text = string.Format(text, args);
    }

    private async void initialize()
    {
        await connectToGraph();
        await Albums_Vw.LoadAlbums();
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

    private void Settings_Click(object? sender, EventArgs e)
    {
        Settings_Vw.IsVisible = true;
        Albums_Vw.IsVisible = false;
    }

    private void Albums_Click(object? sender, EventArgs e)
    {
        Settings_Vw.IsVisible = false;
        Albums_Vw.IsVisible = true;
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
