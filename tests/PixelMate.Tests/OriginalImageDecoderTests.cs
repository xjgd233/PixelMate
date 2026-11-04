using PixelMate.Imaging;

namespace PixelMate.Tests;

public sealed class OriginalImageDecoderTests
{
    private const string Png3By2 =
        "iVBORw0KGgoAAAANSUhEUgAAAAMAAAACCAYAAACddGYaAAAAGUlEQVR4nGPkUbJgULGL+M/AwMDABGOAOAA6tQPcHxf2xgAAAABJRU5ErkJggg==";

    private const string Jpeg3By2 =
        "/9j/4AAQSkZJRgABAQAAAQABAAD/2wBDAAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQH/2wBDAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQH/wAARCAACAAMDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD8t6KKK/0AP4fP/9k=";

    private const string Webp3By2 =
        "UklGRkAAAABXRUJQVlA4IDQAAAAQAgCdASoDAAIAAAAAJZgCdLoB+AH6AAPIAP7+67y/+1KrKtlf/tSz/9ZPvfS6f+sFAAAA";

    private const string TransparentPng1By1 =
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR4nGPgUbJgAAABCwBnKHJnswAAAABJRU5ErkJggg==";

    public static TheoryData<string, string> EncodedFormats =>
        new()
        {
            { Png3By2, ".png" },
            { Jpeg3By2, ".jpg" },
            { Webp3By2, ".webp" }
        };

    [Theory]
    [MemberData(nameof(EncodedFormats))]
    public void DecodesDeclaredEncodedFormats(string base64, string extension)
    {
        RunWithTemporaryImage(
            extension,
            base64,
            image =>
            {
                Assert.Equal(3, image.Width);
                Assert.Equal(2, image.Height);
                Assert.True(image.RowBytes >= image.Width * 4);
            });
    }

    [Fact]
    public void DecodesBmpImage()
    {
        var path = CreateTemporaryPath(".bmp");
        try
        {
            WriteBmp(path, width: 2, height: 2, red: 18, green: 52, blue: 86);

            var image = OriginalImageDecoder.Decode(path);

            Assert.Equal(2, image.Width);
            Assert.Equal(2, image.Height);
            var buffer = ImagePixelBuffer.FromOwnedBgra(
                image.Width,
                image.Height,
                image.BgraPixels,
                image.RowBytes);
            Assert.True(buffer.TryGetPixel(1, 1, out var color));
            Assert.Equal((byte)18, color.Red);
            Assert.Equal((byte)52, color.Green);
            Assert.Equal((byte)86, color.Blue);
            Assert.Equal(byte.MaxValue, color.Alpha);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void PreservesStraightAlphaChannelsFromPng()
    {
        RunWithTemporaryImage(
            ".png",
            TransparentPng1By1,
            image =>
            {
                var buffer = ImagePixelBuffer.FromOwnedBgra(
                    image.Width,
                    image.Height,
                    image.BgraPixels,
                    image.RowBytes);
                Assert.True(buffer.TryGetPixel(0, 0, out var color));
                Assert.Equal(new PixelMate.Colors.PixelColor(12, 34, 56, 0), color);
            });
    }

    [Fact]
    public void RejectsCorruptImageData()
    {
        var path = CreateTemporaryPath(".png");
        try
        {
            File.WriteAllBytes(path, [0x89, 0x50, 0x4E, 0x47, 0x00, 0x01, 0x02]);

            Assert.Throws<InvalidDataException>(
                () => OriginalImageDecoder.Decode(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void DoesNotKeepSourceFileLockedAfterDecode()
    {
        RunWithTemporaryImage(
            ".png",
            Png3By2,
            _ => { },
            deleteSourceBeforeCallback: true);
    }

    private static void RunWithTemporaryImage(
        string extension,
        string base64,
        Action<DecodedOriginalImage> assertImage,
        bool deleteSourceBeforeCallback = false)
    {
        var path = CreateTemporaryPath(extension);
        try
        {
            File.WriteAllBytes(path, Convert.FromBase64String(base64));
            var image = OriginalImageDecoder.Decode(path);
            if (deleteSourceBeforeCallback)
            {
                File.Delete(path);
            }

            assertImage(image);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static string CreateTemporaryPath(string extension)
    {
        return Path.Combine(
            Path.GetTempPath(),
            $"pixelmate-decode-{Guid.NewGuid():N}{extension}");
    }

    private static void WriteBmp(
        string path,
        int width,
        int height,
        byte red,
        byte green,
        byte blue)
    {
        var rowBytes = (width * 3 + 3) & ~3;
        var pixelBytes = rowBytes * height;
        var fileSize = 54 + pixelBytes;

        using var stream = File.Create(path);
        using var writer = new BinaryWriter(stream);
        writer.Write((byte)'B');
        writer.Write((byte)'M');
        writer.Write(fileSize);
        writer.Write((short)0);
        writer.Write((short)0);
        writer.Write(54);
        writer.Write(40);
        writer.Write(width);
        writer.Write(height);
        writer.Write((short)1);
        writer.Write((short)24);
        writer.Write(0);
        writer.Write(pixelBytes);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);

        for (var row = 0; row < height; row++)
        {
            for (var column = 0; column < width; column++)
            {
                writer.Write(blue);
                writer.Write(green);
                writer.Write(red);
            }

            writer.Write(new byte[rowBytes - (width * 3)]);
        }
    }
}
