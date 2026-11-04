using SkiaSharp;

namespace PixelMate.Imaging;

internal sealed class DecodedOriginalImage(
    int width,
    int height,
    int rowBytes,
    byte[] bgraPixels)
{
    public int Width { get; } = width;

    public int Height { get; } = height;

    public int RowBytes { get; } = rowBytes;

    internal byte[] BgraPixels { get; } = bgraPixels;
}

internal static class OriginalImageDecoder
{
    public static DecodedOriginalImage Decode(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        using var codec = SKCodec.Create(filePath)
            ?? throw new InvalidDataException("无法识别图片内容。");
        var sourceInfo = codec.Info;
        if (sourceInfo.Width <= 0 || sourceInfo.Height <= 0)
        {
            throw new InvalidDataException("图片尺寸无效。");
        }

        ImageDecodeLimits.Validate(sourceInfo.Width, sourceInfo.Height);

        // Unpremultiplied BGRA preserves the source RGB channels at low alpha.
        var targetInfo = new SKImageInfo(
            sourceInfo.Width,
            sourceInfo.Height,
            SKColorType.Bgra8888,
            SKAlphaType.Unpremul);
        var pixels = new byte[targetInfo.BytesSize];
        var result = codec.GetPixels(targetInfo, pixels);
        if (result != SKCodecResult.Success)
        {
            throw new InvalidDataException($"图片像素解码失败：{result}。");
        }

        return new DecodedOriginalImage(
            targetInfo.Width,
            targetInfo.Height,
            targetInfo.RowBytes,
            pixels);
    }
}
