using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace OneDriveAlbums.UI;

public sealed class AppConfig
{
    public AppConfig() { }

    public static AppConfig LoadFromJson(string json)
    {
        AppConfig? result = JsonSerializer.Deserialize<AppConfig>(json, options);
        if (result == null)
        {
            return new();
        }
        result.IgnoredPathRegexs = result.ignoredPaths.Select(path => new Regex(path)).ToArray();
        return result;
    }

    public Regex[] IgnoredPathRegexs { get; set; } = [];
    public int MaxElements { get; set; } = int.MaxValue;
    public int DuplicatesStartSearchFrom { get; set; } = 1;
    public bool AlwaysEqual { get; set; } = false;
    public bool DryRun { get; set; } = false;
    public string[] ignoredPaths { get; set; } = [];

    private static JsonSerializerOptions options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };
}

public partial class App : Application
{
    private const string ConfigFileName = "config.json";

    public static AppConfig Config { get; private set; } = new();

    public App()
    {
        StartupLog.Write("App:.ctor begin");

        try
        {
            loadConfig();
            StartupLog.Write("App:loadConfig ok");
        }
        catch (Exception ex)
        {
            StartupLog.Write(ex, "App:loadConfig failed");
            throw;
        }

        try
        {
            loadUILanguage();
            StartupLog.Write("App:loadUILanguage ok");
        }
        catch (Exception ex)
        {
            StartupLog.Write(ex, "App:loadUILanguage failed");
            throw;
        }

        try
        {
            InitializeComponent();
            StartupLog.Write("App:InitializeComponent ok");
        }
        catch (Exception ex)
        {
            StartupLog.Write(ex, "App:InitializeComponent failed");
            throw;
        }

        StartupLog.Write($"App:.ctor end. LogPath={StartupLog.LogPath}");
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        StartupLog.Write("App:CreateWindow begin");

        try
        {
#if IOS
            // Workaround for iOS AOT crash in Shell flyout cell creation:
            // Microsoft.Maui.Controls.Platform.Compatibility.ShellTableViewSource.GetCell -> Grid.DefinitionsChanged (DynamicMethod).
            var window = new Window(new MainPage());
            StartupLog.Write("App:CreateWindow end (MainPage created, no Shell)");
            return window;
#else
            var window = new Window(new AppShell());
            StartupLog.Write("App:CreateWindow end (AppShell created)");
            return window;
#endif
        }
        catch (Exception ex)
        {
            StartupLog.Write(ex, "App:CreateWindow failed");
            throw;
        }
    }

    private static void loadUILanguage()
    {
        string persistedLang = Preferences.Get("AppCulture", "en-US");
        CultureInfo culture = new(persistedLang);
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
    }

    private static void loadConfig()
    {
        try
        {
            string configDir = Path.Combine(FileSystem.AppDataDirectory, ".OneDriveManager");
            Directory.CreateDirectory(configDir);
            string path = Path.Combine(configDir, ConfigFileName);

            string json = File.ReadAllText(path);
            Config = AppConfig.LoadFromJson(json);
        }
        catch
        {
            Config = new();
        }
    }

}