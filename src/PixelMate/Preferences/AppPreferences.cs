using System.Text.Json;

namespace PixelMate.Preferences;

public sealed class AppPreferences
{
    private static readonly int[] SupportedMagnifierScales = [8, 12, 16];
    private readonly string _settingsPath;
    private PreferencesData _data;

    public AppPreferences(string settingsPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(settingsPath);
        _settingsPath = settingsPath;
        _data = Normalize(Read(settingsPath));
    }

    public string? LastOpenDirectory => _data.LastOpenDirectory;

    public bool RememberLastDirectory => _data.RememberLastDirectory;

    public bool ShowTutorialEveryLaunch => _data.ShowTutorialEveryLaunch;

    public int MagnifierScale => _data.MagnifierScale;

    public bool ShouldShowTutorial =>
        !_data.HasSeenTutorial || _data.ShowTutorialEveryLaunch;

    public static AppPreferences CreateDefault()
    {
        var applicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return new AppPreferences(Path.Combine(applicationData, "PixelMate", "settings.json"));
    }

    public void RememberDirectory(string filePath)
    {
        if (!RememberLastDirectory)
        {
            return;
        }

        var directory = Path.GetDirectoryName(filePath);
        if (string.IsNullOrWhiteSpace(directory))
        {
            return;
        }

        _data = _data with { LastOpenDirectory = directory };
        Save();
    }

    public AppSettingsSnapshot CreateSettingsSnapshot()
    {
        return new AppSettingsSnapshot(
            RememberLastDirectory,
            ShowTutorialEveryLaunch,
            MagnifierScale);
    }

    public void ApplySettings(AppSettingsSnapshot settings)
    {
        _data = Normalize(_data with
        {
            RememberLastDirectory = settings.RememberLastDirectory,
            ShowTutorialEveryLaunch = settings.ShowTutorialEveryLaunch,
            MagnifierScale = settings.MagnifierScale
        });
        Save();
    }

    public void MarkTutorialSeen()
    {
        if (_data.HasSeenTutorial)
        {
            return;
        }

        _data = _data with { HasSeenTutorial = true };
        Save();
    }

    private void Save()
    {
        string? temporaryPath = null;
        try
        {
            var parentDirectory = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrWhiteSpace(parentDirectory))
            {
                Directory.CreateDirectory(parentDirectory);
            }

            temporaryPath = $"{_settingsPath}.{Guid.NewGuid():N}.tmp";
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(_data));
            File.Move(temporaryPath, _settingsPath, overwrite: true);
            temporaryPath = null;
        }
        catch (IOException)
        {
            // A read-only profile must not prevent the image viewer from working.
        }
        catch (UnauthorizedAccessException)
        {
            // A read-only profile must not prevent the image viewer from working.
        }
        finally
        {
            if (temporaryPath is not null)
            {
                try
                {
                    File.Delete(temporaryPath);
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }
    }

    private static PreferencesData Normalize(PreferencesData data)
    {
        return SupportedMagnifierScales.Contains(data.MagnifierScale)
            ? data
            : data with { MagnifierScale = 12 };
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

        public bool RememberLastDirectory { get; init; } = true;

        public bool HasSeenTutorial { get; init; }

        public bool ShowTutorialEveryLaunch { get; init; }

        public int MagnifierScale { get; init; } = 12;
    }
}

public readonly record struct AppSettingsSnapshot(
    bool RememberLastDirectory,
    bool ShowTutorialEveryLaunch,
    int MagnifierScale);
