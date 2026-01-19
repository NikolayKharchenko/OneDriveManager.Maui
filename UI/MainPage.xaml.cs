
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
    }

    public void SetStatusText(string text = "", params object[] args)
    {
        Status_Lbl.Text = string.Format(text, args);
    }

    private void SignIn_Clicked(object sender, EventArgs e)
    {
        connectToGraph();
    }

    private async void connectToGraph()
    {
        try
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
        catch (Exception ex)
        {
            SetStatusText(ex.Message);
        }
    }

    private void ViewAlbums_Clicked(object sender, EventArgs e)
    {

    }
    private async void SignOut_Clicked(object sender, EventArgs e)
    {
        if (GraphClient.Instance.IsConnected)
        {
            await GraphClient.Instance.Disconnect();
            SetStatusText(Strings.Ready_Txt);
        }
    }
}
