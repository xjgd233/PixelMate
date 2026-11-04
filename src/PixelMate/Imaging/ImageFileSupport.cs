namespace PixelMate.Imaging;

public static class ImageFileSupport
{
    public static IReadOnlyList<string> Extensions { get; } =
        [".png", ".jpg", ".jpeg", ".bmp", ".webp"];

    public static IReadOnlyList<string> Patterns { get; } =
        ["*.png", "*.jpg", "*.jpeg", "*.bmp", "*.webp"];

    public static bool IsSupportedPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var extension = Path.GetExtension(path);
        return Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }
}
