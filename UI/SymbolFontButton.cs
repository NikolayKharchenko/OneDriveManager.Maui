namespace OneDriveAlbums.UI;


public class SymbolFontButton : Label
{
#if WINDOWS
    Dictionary<string, string> ligatureToGlyph = new()
    {
        { "cloud_off", "\uE753" },
        { "settings", "\uE713" },
        { "close", "\uE711" },
        { "arrow_drop_up", "\uEDDB" },
        { "arrow_drop_down", "\uEDDC" }
    };
#endif
    
    public event EventHandler? Clicked;

    public SymbolFontButton()
	{
#if WINDOWS
        FontFamily = "Segoe Fluent Icons";
#else
        FontFamily = "MaterialSymbolsRounded";
#endif
        TapGestureRecognizer tapGesture = new();
        tapGesture.Tapped += (_, _) => Clicked?.Invoke(this, EventArgs.Empty);
        GestureRecognizers.Add(tapGesture);
    }

    public string Ligature
    {
        get => throw new NotImplementedException();
#if WINDOWS
        set => Text = ligatureToGlyph.GetValueOrDefault(value, "");
#else
		set => Text = value;
#endif
    }

}
