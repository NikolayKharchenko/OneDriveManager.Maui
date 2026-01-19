using Microsoft.Graph.Models;
using OneDriveAlbums.Graph;
using OneDriveAlbums.UI.Resources.Strings;
using System.Diagnostics;

namespace OneDriveAlbums.UI;

public partial class MainPage : ContentPage
{
    private const string ClientId = "14667aef-1076-4332-a2aa-de72d31a570f";

    static private MainPage? instance;
    static public MainPage Instance
    {
        get
        {
            Trace.Assert(instance != null, "MainPage instance is null. Make sure to access MainPage.Instance after the MainPage has been constructed.");
            return instance;
        }
    }
    public MainPage()
    {
        instance = this;
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

        string baseDir = Platform.GetOneDriveLocalDirectory();
        AuthProviderImplBase auth = Platform.CreateAuthProvider(ClientId);

        await GraphClient.Instance.Connect(
            new GraphClient.Config(
                OneDriveRootFolder: baseDir,
                MaxElements: App.Config.MaxElements),
            auth);

        string displayName = await GraphClient.Instance.ConnectedAccountName();
        SetStatusText(Strings.Connected_Msg, displayName);
    }
}
