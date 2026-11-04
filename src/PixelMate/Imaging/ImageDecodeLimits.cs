namespace PixelMate.Imaging;

public static class ImageDecodeLimits
{
    public const int MaximumDimension = 32767;
    public const long MaximumPixelCount = 64_000_000;

    public static void Validate(int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            throw new InvalidDataException("图片尺寸无效。");
        }

        if (width > MaximumDimension || height > MaximumDimension)
        {
            throw new InvalidDataException(
                $"图片边长不能超过 {MaximumDimension} 像素。");
        }

        if ((long)width * height > MaximumPixelCount)
        {
            throw new InvalidDataException(
                $"图片不能超过 {MaximumPixelCount / 1_000_000} 百万像素。");
        }
    }
}
