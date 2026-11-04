using PixelMate.Colors;

namespace PixelMate.Imaging;

public sealed class ImagePixelBuffer
{
    private const int BytesPerPixel = 4;

    private readonly byte[] _pixels;

    public ImagePixelBuffer(
        int width,
        int height,
        ReadOnlySpan<byte> pixels,
        int rowBytes)
        : this(width, height, rowBytes, CopyValidated(width, height, pixels, rowBytes))
    {
    }

    private ImagePixelBuffer(int width, int height, int rowBytes, byte[] pixels)
    {
        Width = width;
        Height = height;
        RowBytes = rowBytes;
        _pixels = pixels;
    }

    internal static ImagePixelBuffer FromOwnedBgra(
        int width,
        int height,
        byte[] pixels,
        int rowBytes)
    {
        Validate(width, height, rowBytes, pixels.Length);
        return new ImagePixelBuffer(width, height, rowBytes, pixels);
    }

    private static byte[] CopyValidated(
        int width,
        int height,
        ReadOnlySpan<byte> pixels,
        int rowBytes)
    {
        Validate(width, height, rowBytes, pixels.Length);
        return pixels[..checked(rowBytes * height)].ToArray();
    }

    private static void Validate(int width, int height, int rowBytes, int length)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "图片宽度必须大于零。");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "图片高度必须大于零。");
        }

        var minimumRowBytes = checked(width * BytesPerPixel);
        if (rowBytes < minimumRowBytes)
        {
            throw new ArgumentOutOfRangeException(
                nameof(rowBytes),
                "每行字节数小于图片宽度所需空间。");
        }

        if (length < checked(rowBytes * height))
        {
            throw new ArgumentException("像素数据长度不足。", "pixels");
        }

    }

    public int Width { get; }

    public int Height { get; }

    public int RowBytes { get; }

    public bool TryGetPixel(int x, int y, out PixelColor color)
    {
        color = default;
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return false;
        }

        var offset = checked((y * RowBytes) + (x * BytesPerPixel));
        color = new PixelColor(
            _pixels[offset + 2],
            _pixels[offset + 1],
            _pixels[offset],
            _pixels[offset + 3]);
        return true;
    }
}
