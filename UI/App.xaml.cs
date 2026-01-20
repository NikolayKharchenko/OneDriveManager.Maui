using System.Text.Json;
using System.Text.RegularExpressions;

namespace OneDriveAlbums.UI;

public sealed class AppConfig
{
    public AppConfig() { }

    public AppConfig(string json)
    {
        AppConfigJson conf = JsonSerializer.Deserialize<AppConfigJson>(json, options);
        IgnoredPaths = conf.IgnoredPaths.Select(path => new Regex(path)).ToArray();
        MaxElements = int.Max(conf.MaxElements, 1);
        DuplicatesStartSearchFrom = int.Max(conf.DuplicatesStartSearchFrom, 1);
        AlwaysEqual = conf.AlwaysEqual;
        DryRun = conf.DryRun;
    }

    public Regex[] IgnoredPaths { get; set; } = [];
    public int MaxElements { get; set; } = int.MaxValue;
    public int DuplicatesStartSearchFrom { get; set; } = 1;
    public bool AlwaysEqual { get; set; } = false;
    public bool DryRun { get; set; } = false;

    private static JsonSerializerOptions options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };
}

public record struct AppConfigJson(
    string[] IgnoredPaths,
    int MaxElements,
    int DuplicatesStartSearchFrom,
    bool AlwaysEqual,
    bool DryRun
);

public partial class App : Application
{
    private const string ConfigFileName = "config.json";

    public static AppConfig Config { get; private set; } = new();

    public App()
    {
        LoadConfig();
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }

    private static void LoadConfig()
    {
        try
        {
            string baseDir =
#if ANDROID
                FileSystem.AppDataDirectory;
#else
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
#endif

            string configDir = Path.Combine(baseDir, ".OneDriveManager");
            Directory.CreateDirectory(configDir);
            string path = Path.Combine(configDir, ConfigFileName);

            string json = File.ReadAllText(path);
            Config = new(json);
        }
        catch
        {
            Config = new();
        }
    }

}