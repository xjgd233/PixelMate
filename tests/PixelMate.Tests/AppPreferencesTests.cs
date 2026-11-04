using PixelMate.Preferences;

namespace PixelMate.Tests;

public class AppPreferencesTests
{
    [Fact]
    public void RememberDirectory_PersistsParentDirectory()
    {
        var temporaryDirectory = CreateTemporaryDirectory();
        try
        {
            var settingsPath = Path.Combine(temporaryDirectory, "settings.json");
            var imagePath = Path.Combine(temporaryDirectory, "sample image.png");
            var preferences = new AppPreferences(settingsPath);

            preferences.RememberDirectory(imagePath);
            var reloaded = new AppPreferences(settingsPath);

            Assert.Equal(temporaryDirectory, preferences.LastOpenDirectory);
            Assert.Equal(temporaryDirectory, reloaded.LastOpenDirectory);
        }
        finally
        {
            Directory.Delete(temporaryDirectory, true);
        }
    }

    [Fact]
    public void Constructor_IgnoresMalformedSettings()
    {
        var temporaryDirectory = CreateTemporaryDirectory();
        try
        {
            var settingsPath = Path.Combine(temporaryDirectory, "settings.json");
            File.WriteAllText(settingsPath, "{not valid json");

            var preferences = new AppPreferences(settingsPath);

            Assert.Null(preferences.LastOpenDirectory);
        }
        finally
        {
            Directory.Delete(temporaryDirectory, true);
        }
    }

    [Fact]
    public void RememberDirectory_ReplacesSettingsWithoutLeavingTemporaryFiles()
    {
        var temporaryDirectory = CreateTemporaryDirectory();
        try
        {
            var settingsPath = Path.Combine(temporaryDirectory, "settings.json");
            File.WriteAllText(settingsPath, "{\"LastOpenDirectory\":\"/old\"}");
            var imagePath = Path.Combine(temporaryDirectory, "new.png");
            var preferences = new AppPreferences(settingsPath);

            preferences.RememberDirectory(imagePath);

            Assert.Equal(
                temporaryDirectory,
                new AppPreferences(settingsPath).LastOpenDirectory);
            Assert.Empty(
                Directory.GetFiles(
                    temporaryDirectory,
                    "settings.json.*.tmp"));
        }
        finally
        {
            Directory.Delete(temporaryDirectory, true);
        }
    }

    [Fact]
    public void NewPreferences_ShowTutorialOnFirstLaunch()
    {
        var temporaryDirectory = CreateTemporaryDirectory();
        try
        {
            var preferences = new AppPreferences(
                Path.Combine(temporaryDirectory, "settings.json"));

            Assert.True(preferences.ShouldShowTutorial);
            Assert.False(preferences.ShowTutorialEveryLaunch);
            Assert.Equal(12, preferences.MagnifierScale);
        }
        finally
        {
            Directory.Delete(temporaryDirectory, true);
        }
    }

    [Fact]
    public void MarkTutorialSeen_PersistsCompletion()
    {
        var temporaryDirectory = CreateTemporaryDirectory();
        try
        {
            var settingsPath = Path.Combine(temporaryDirectory, "settings.json");
            var preferences = new AppPreferences(settingsPath);

            preferences.MarkTutorialSeen();
            var reloaded = new AppPreferences(settingsPath);

            Assert.False(reloaded.ShouldShowTutorial);
        }
        finally
        {
            Directory.Delete(temporaryDirectory, true);
        }
    }

    [Fact]
    public void ApplySettings_PersistsSupportedPreferences()
    {
        var temporaryDirectory = CreateTemporaryDirectory();
        try
        {
            var settingsPath = Path.Combine(temporaryDirectory, "settings.json");
            var preferences = new AppPreferences(settingsPath);

            preferences.ApplySettings(
                new AppSettingsSnapshot(
                    RememberLastDirectory: false,
                    ShowTutorialEveryLaunch: true,
                    MagnifierScale: 16));
            var reloaded = new AppPreferences(settingsPath);

            Assert.False(reloaded.RememberLastDirectory);
            Assert.True(reloaded.ShowTutorialEveryLaunch);
            Assert.True(reloaded.ShouldShowTutorial);
            Assert.Equal(16, reloaded.MagnifierScale);
        }
        finally
        {
            Directory.Delete(temporaryDirectory, true);
        }
    }

    [Fact]
    public void RememberDirectory_DoesNotPersistWhenDisabled()
    {
        var temporaryDirectory = CreateTemporaryDirectory();
        try
        {
            var settingsPath = Path.Combine(temporaryDirectory, "settings.json");
            var preferences = new AppPreferences(settingsPath);
            preferences.ApplySettings(
                new AppSettingsSnapshot(
                    RememberLastDirectory: false,
                    ShowTutorialEveryLaunch: false,
                    MagnifierScale: 12));

            preferences.RememberDirectory(
                Path.Combine(temporaryDirectory, "sample.png"));

            Assert.Null(new AppPreferences(settingsPath).LastOpenDirectory);
        }
        finally
        {
            Directory.Delete(temporaryDirectory, true);
        }
    }

    [Fact]
    public void UnsupportedMagnifierScale_FallsBackToDefault()
    {
        var temporaryDirectory = CreateTemporaryDirectory();
        try
        {
            var settingsPath = Path.Combine(temporaryDirectory, "settings.json");
            File.WriteAllText(settingsPath, "{\"MagnifierScale\":99}");

            var preferences = new AppPreferences(settingsPath);

            Assert.Equal(12, preferences.MagnifierScale);
        }
        finally
        {
            Directory.Delete(temporaryDirectory, true);
        }
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"pixelmate-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}
