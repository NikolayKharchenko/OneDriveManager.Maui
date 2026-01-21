using System.Globalization;

namespace OneDriveAlbums.UI;



public partial class SettingsView : ContentView
{
    public List<CultureInfo> SupportedCultures { get; } = new()
    {
        new CultureInfo("en-US"),
        new CultureInfo("ru"),
    };

    private int initialLanguageIndex;

    public SettingsView()
	{
		InitializeComponent();

        InterfaceLanguage_Picker.ItemsSource = SupportedCultures.Select(c => c.NativeName).ToList();
        CultureInfo currentCulture = Thread.CurrentThread.CurrentUICulture;
        initialLanguageIndex = SupportedCultures.FindIndex(c => c.Name == currentCulture.Name);
        InterfaceLanguage_Picker.SelectedIndex = initialLanguageIndex;

    }

    private void FixDates_Click(object sender, EventArgs e)
    {

    }

    private void InterfaceLanguage_Changed(object sender, EventArgs e)
    {
        if (InterfaceLanguage_Picker.SelectedIndex < 0 || InterfaceLanguage_Picker.SelectedIndex >= SupportedCultures.Count)
            return;

        CultureInfo selectedCulture = SupportedCultures[InterfaceLanguage_Picker.SelectedIndex];
        Preferences.Set("AppCulture", selectedCulture.Name);

        InterfaceLanguageChanged_Warn.IsVisible = InterfaceLanguage_Picker.SelectedIndex != initialLanguageIndex;
    }
}