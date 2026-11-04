using System.Text.Json;

namespace PixelMate.Preferences;

public sealed class AppPreferences
{
    private readonly string _settingsPath;
    private PreferencesData _data;

    public AppPreferences(string settingsPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(settingsPath);
        _settingsPath = settingsPath;
        _data = Read(settingsPath);
    }

    public string? LastOpenDirectory => _data.LastOpenDirectory;

    public static AppPreferences CreateDefault()
    {
        var applicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return new AppPreferences(Path.Combine(applicationData, "PixelMate", "settings.json"));
    }

    public void RememberDirectory(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (string.IsNullOrWhiteSpace(directory))
        {
            return;
        }

        _data = _data with { LastOpenDirectory = directory };

        try
        {
            var parentDirectory = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrWhiteSpace(parentDirectory))
            {
                Directory.CreateDirectory(parentDirectory);
            }

            File.WriteAllText(
                _settingsPath,
                JsonSerializer.Serialize(_data));
        }
        catch (IOException)
        {
            // A read-only profile must not prevent the image viewer from working.
        }
        catch (UnauthorizedAccessException)
        {
            // A read-only profile must not prevent the image viewer from working.
        }
    }

    private static PreferencesData Read(string settingsPath)
    {
        try
        {
            if (!File.Exists(settingsPath))
            {
                return new PreferencesData();
            }

            var json = File.ReadAllText(settingsPath);
            return JsonSerializer.Deserialize<PreferencesData>(json)
                ?? new PreferencesData();
        }
        catch (IOException)
        {
            return new PreferencesData();
        }
        catch (UnauthorizedAccessException)
        {
            return new PreferencesData();
        }
        catch (JsonException)
        {
            return new PreferencesData();
        }
    }

    internal sealed record PreferencesData
    {
        public string? LastOpenDirectory { get; init; }
    }
}
