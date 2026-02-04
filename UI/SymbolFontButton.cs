namespace OneDriveAlbums.UI;


public class SymbolFontButton : Label
{
    static private readonly Dictionary<string, string> ligatureToGlyph = new()
    {
        { "cloud_off", "\uE753" },
        { "cloud", "\uE753" },
        { "photo_collection", "\uE7AA" },
        { "settings", "\uE713" },
        { "close", "\uE711" },
        { "arrow_drop_up", "\uEDDB" },
        { "arrow_drop_down", "\uEDDC" }
    };
    
    public event EventHandler? Clicked;

    public SymbolFontButton()
	{
        FontFamily = "SegoeFluentIcons";
        TapGestureRecognizer tapGesture = new();
        tapGesture.Tapped += (_, _) => Clicked?.Invoke(this, EventArgs.Empty);
        GestureRecognizers.Add(tapGesture);
    }

    public string Ligature
    {
        get => ligatureToGlyph.FirstOrDefault(x => x.Value == Text).Key ?? "";
        set => Text = ligatureToGlyph.GetValueOrDefault(value, "");
    }

}
