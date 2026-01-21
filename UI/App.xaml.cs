using System.Linq;
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
            Config = AppConfig.LoadFromJson(json);
        }
        catch
        {
            Config = new();
        }
    }

}