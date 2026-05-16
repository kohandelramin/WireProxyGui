using System.IO;
using System.Text.Json;

namespace WireProxyGui.Services;

public sealed class AppSettings
{
    public string ConfigPath { get; set; } = string.Empty;
    public string HttpIp { get; set; } = "127.0.0.1";
    public int HttpPort { get; set; } = 25345;
    public string SocksIp { get; set; } = "127.0.0.1";
    public int SocksPort { get; set; } = 25344;
}

public sealed class SettingsService
{
    private readonly string _settingsPath;

    public SettingsService(string baseDirectory)
    {
        _settingsPath = Path.Combine(baseDirectory, "wireproxy-gui-settings.json");
    }

    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                return new AppSettings();
            }

            var json = File.ReadAllText(_settingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(_settingsPath, json);
    }
}
