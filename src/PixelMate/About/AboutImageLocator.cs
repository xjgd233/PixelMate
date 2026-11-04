namespace PixelMate.About;

internal static class AboutImageLocator
{
    public static string? FindPicture(
        string picturesDirectory,
        IReadOnlyList<string> keywords)
    {
        if (string.IsNullOrWhiteSpace(picturesDirectory)
            || !Directory.Exists(picturesDirectory))
        {
            return null;
        }

        try
        {
            return Directory
                .EnumerateFiles(picturesDirectory)
                .Where(IsSupportedImage)
                .Where(path =>
                {
                    var name = Path.GetFileNameWithoutExtension(path);
                    return keywords.Any(keyword =>
                        name.Contains(keyword, StringComparison.OrdinalIgnoreCase));
                })
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault();
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static bool IsSupportedImage(string path)
    {
        var extension = Path.GetExtension(path);
        return extension.Equals(".png", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".webp", StringComparison.OrdinalIgnoreCase);
    }
}
