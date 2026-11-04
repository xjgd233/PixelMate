using PixelMate.About;

namespace PixelMate.Tests;

public class AboutImageLocatorTests
{
    [Fact]
    public void FindPicture_MatchesKeywordAndSupportedExtension()
    {
        var temporaryDirectory = CreateTemporaryDirectory();
        try
        {
            var avatarPath = Path.Combine(temporaryDirectory, "PixelMate-Avatar.JPG");
            File.WriteAllBytes(avatarPath, [0x01]);
            File.WriteAllBytes(
                Path.Combine(temporaryDirectory, "avatar.txt"),
                [0x02]);

            var result = AboutImageLocator.FindPicture(
                temporaryDirectory,
                ["avatar", "头像"]);

            Assert.Equal(avatarPath, result);
        }
        finally
        {
            Directory.Delete(temporaryDirectory, true);
        }
    }

    [Fact]
    public void FindPicture_ReturnsNullWhenNoKeywordMatches()
    {
        var temporaryDirectory = CreateTemporaryDirectory();
        try
        {
            File.WriteAllBytes(
                Path.Combine(temporaryDirectory, "wallpaper.png"),
                [0x01]);

            var result = AboutImageLocator.FindPicture(
                temporaryDirectory,
                ["sponsor", "赞助"]);

            Assert.Null(result);
        }
        finally
        {
            Directory.Delete(temporaryDirectory, true);
        }
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"pixelmate-about-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}
