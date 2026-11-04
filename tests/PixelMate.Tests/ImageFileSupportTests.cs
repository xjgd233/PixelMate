using PixelMate.Imaging;

namespace PixelMate.Tests;

public class ImageFileSupportTests
{
    [Theory]
    [InlineData("photo.png")]
    [InlineData("photo.JPG")]
    [InlineData("photo.jpeg")]
    [InlineData("photo.BMP")]
    [InlineData("photo.webp")]
    public void IsSupportedPath_AcceptsDeclaredImageFormats(string path)
    {
        Assert.True(ImageFileSupport.IsSupportedPath(path));
    }

    [Theory]
    [InlineData("photo.gif")]
    [InlineData("photo.svg")]
    [InlineData("notes.txt")]
    [InlineData("")]
    [InlineData(null)]
    public void IsSupportedPath_RejectsUnsupportedPaths(string? path)
    {
        Assert.False(ImageFileSupport.IsSupportedPath(path));
    }
}
