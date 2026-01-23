using OneDriveAlbums.UI.Resources.Strings;
using System.Globalization;
using System.Resources;
namespace OneDriveAlbums.UI;

public partial class SettingsView : ContentView
{
    public static List<CultureInfo> GetAvailableResourceCultures()
    {
        ResourceManager resourceManager = Strings.ResourceManager;
        List<CultureInfo> cultures = new();

        foreach (var culture in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
        {
            if (culture == CultureInfo.InvariantCulture)
                continue;
            try
            {
                // Only add if a resource set exists and is not just the neutral fallback
                ResourceSet? set = resourceManager.GetResourceSet(culture, true, false);
                if (set != null)
                    cultures.Add(culture);
            }
            catch { /* Ignore cultures that throw */ }
        }

        return cultures;
    }

    public SettingsView()
	{
		InitializeComponent();

        InterfaceLanguage_Picker.ItemsSource = GetAvailableResourceCultures();
        InterfaceLanguage_Picker.SelectedItem = Thread.CurrentThread.CurrentUICulture;
        InterfaceLanguageChanged_Warn.IsVisible = false;
    }

    private void FixDates_Click(object? sender, EventArgs e)
    {
        Dispatcher.Dispatch(async () => await MainPage.Instance!.Albums_Vw.FixAllAlbumsMetadataAsync());
    }

    private void InterfaceLanguage_Changed(object? sender, EventArgs e)
    {
        if (InterfaceLanguage_Picker.SelectedItem is null)
            return;

        CultureInfo selectedCulture = (CultureInfo)InterfaceLanguage_Picker.SelectedItem;
        Preferences.Set("AppCulture", selectedCulture.Name);

        InterfaceLanguageChanged_Warn.IsVisible = true;
    }
}