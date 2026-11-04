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

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"pixelmate-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}
