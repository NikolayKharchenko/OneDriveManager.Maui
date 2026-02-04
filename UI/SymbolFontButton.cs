namespace OneDriveAlbums.UI;


public class SymbolFontButton : Label
{
    public event EventHandler? Clicked;

    public SymbolFontButton()
	{
        FontFamily = "MaterialSymbolsRounded";
        TapGestureRecognizer tapGesture = new();
        tapGesture.Tapped += (_, _) => Clicked?.Invoke(this, EventArgs.Empty);
        GestureRecognizers.Add(tapGesture);
    }
}
