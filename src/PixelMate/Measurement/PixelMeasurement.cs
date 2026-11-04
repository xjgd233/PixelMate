using PixelMate.Imaging;
using System.Globalization;
using System.Text;

namespace PixelMate.Measurement;

public readonly record struct PixelMeasurement(ImagePoint Start, ImagePoint End)
{
    public double DeltaX => Math.Abs(End.X - Start.X);

    public double DeltaY => Math.Abs(End.Y - Start.Y);

    public double Length => Math.Sqrt((DeltaX * DeltaX) + (DeltaY * DeltaY));

    public PixelMeasurement ConstrainToAxis()
    {
        var deltaX = End.X - Start.X;
        var deltaY = End.Y - Start.Y;
        return Math.Abs(deltaX) >= Math.Abs(deltaY)
            ? new PixelMeasurement(Start, new ImagePoint(End.X, Start.Y))
            : new PixelMeasurement(Start, new ImagePoint(Start.X, End.Y));
    }
}

public static class MeasurementFormatting
{
    public static string FormatLabel(PixelMeasurement measurement)
    {
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{FormatNumber(measurement.Length)}px");
    }

    public static string FormatClipboard(IReadOnlyList<PixelMeasurement> measurements)
    {
        ArgumentNullException.ThrowIfNull(measurements);

        var builder = new StringBuilder();
        for (var index = 0; index < measurements.Count; index++)
        {
            if (index > 0)
            {
                builder.AppendLine();
            }

            var measurement = measurements[index];
            builder.Append(CultureInfo.InvariantCulture, $"{index + 1}. ");
            builder.Append(CultureInfo.InvariantCulture,
                $"L {FormatNumber(measurement.Length)} px | ");
            builder.Append(CultureInfo.InvariantCulture,
                $"ΔX {FormatNumber(measurement.DeltaX)} | ");
            builder.Append(CultureInfo.InvariantCulture,
                $"ΔY {FormatNumber(measurement.DeltaY)} | ");
            builder.Append(CultureInfo.InvariantCulture,
                $"({FormatNumber(measurement.Start.X)}, {FormatNumber(measurement.Start.Y)}) → ");
            builder.Append(CultureInfo.InvariantCulture,
                $"({FormatNumber(measurement.End.X)}, {FormatNumber(measurement.End.Y)})");
        }

        return builder.ToString();
    }

    private static string FormatNumber(double value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }
}
