
using OneDriveAlbums.UI.Resources.Strings;
using System.Diagnostics;

namespace OneDriveAlbums.UI;

public partial class MainPage : ContentPage
{
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

    public void SetStatusText(string text = "")
    {
        Status_Lbl.Text = text;
    }

}
