namespace PixelMate.Imaging;

public static class EditedImageExport
{
    public static double CalculateOverlayScale(int width, int height)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width));
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height));
        }

        return Math.Clamp(Math.Min(width, height) / 900d, 1d, 3d);
    }

    public static string CreateSuggestedFileName(string? sourcePath)
    {
        var baseName = string.IsNullOrWhiteSpace(sourcePath)
            ? "PixelMate"
            : Path.GetFileNameWithoutExtension(sourcePath);
        return $"{(string.IsNullOrWhiteSpace(baseName) ? "PixelMate" : baseName)}-已编辑.png";
    }
}
