using System.ComponentModel;
using SkiaSharp;

namespace X39.Solutions.PdfTemplate.Data;

/// <summary>
/// Represents a color.
/// </summary>
/// <param name="Red">The red component of the color.</param>
/// <param name="Green">The green component of the color.</param>
/// <param name="Blue">The blue component of the color.</param>
/// <param name="Alpha">The alpha component of the color.</param>
[TypeConverter(typeof(ColorConverter))]
[PublicAPI]
public readonly record struct Color(byte Red, byte Green, byte Blue, byte Alpha = 255) : ISpanParsable<Color>
{
    /// <summary>
    /// Creates a new color.
    /// </summary>
    /// <param name="red">The red component of the color.</param>
    /// <param name="green">The green component of the color.</param>
    /// <param name="blue">The blue component of the color.</param>
    /// <param name="alpha">The alpha component of the color.</param>
    public Color(float red, float green, float blue, float alpha = 1.0F) : this(
        (byte) (red * 255),
        (byte) (green * 255),
        (byte) (blue * 255),
        (byte) (alpha * 255))
    {
    }

    /// <summary>
    /// Creates a new color.
    /// </summary>
    /// <param name="rgba">The color as a uint.</param>
    public Color(uint rgba) : this(
        (byte) ((rgba >> 24) & 0xFF),
        (byte) ((rgba >> 16) & 0xFF),
        (byte) ((rgba >> 8) & 0xFF),
        (byte) (rgba & 0xFF))
    {
    }

    /// <summary>
    /// Converts the color into the HSL color space.
    /// </summary>
    /// <returns>The color in the HSL color space.</returns>
    // ReSharper disable once InconsistentNaming
    public (ushort hue, float saturation, float light, float alpha) ToHSL()
    {
        var r = Red;
        var g = Green;
        var b = Blue;

        var r1 = r / 255.0F;
        var g1 = g / 255.0F;
        var b1 = b / 255.0F;
        // ReSharper disable once IdentifierTypo
        var cmax = Math.Max(r1, Math.Max(g1, b1));
        // ReSharper disable once IdentifierTypo
        var cmin = Math.Min(r1, Math.Min(g1, b1));
        var delta = cmax - cmin;
        var h = 0.0F;
        if (delta != 0.0F)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (cmax == r1)
                h = ((g1 - b1) / delta) % 6.0F;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            else if (cmax == g1)
                h = ((b1 - r1) / delta) + 2.0F;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            else if (cmax == b1)
                h = ((r1 - g1) / delta) + 4.0F;
        }

        h *= 60.0F;

        var l = (cmax + cmin) / 2.0F;

        var s = 0.0F;
        if (delta != 0.0F)
            s = delta / (1.0F - Math.Abs(2.0F * l - 1.0F));

        return ((ushort) (h % 360), s, l, Alpha / 255.0F);
    }

    /// <summary>
    /// Creates a color from the HSL color space. 
    /// </summary>
    /// <param name="hue">The hue of the color.</param>
    /// <param name="saturation">The saturation of the color.</param>
    /// <param name="light">The light of the color.</param>
    /// <param name="alpha">The alpha of the color.</param>
    /// <returns>The color in the RGB color space.</returns>
    // ReSharper disable once InconsistentNaming
    public static Color FromHSL([ValueRange(0, 360)] ushort hue, float saturation, float light, byte alpha = 255)
    {
        var l = light;
        var s = saturation;
        var h = hue;
        var c = (1.0F - Math.Abs(2.0F * l - 1.0F)) * s;
        var x = c * (1.0F - Math.Abs(h / 60.0F % 2.0F - 1.0F));
        var m = l - c / 2.0F;
        var (r1, g1, b1) = h switch
        {
            < 60  => (c, x, 0.0F),
            < 120 => (x, c, 0.0F),
            < 180 => (0.0F, c, x),
            < 240 => (0.0F, x, c),
            < 300 => (x, 0.0F, c),
            _     => (c, 0.0F, x),
        };

        var r = (byte) ((r1 + m) * 255.0F);
        var g = (byte) ((g1 + m) * 255.0F);
        var b = (byte) ((b1 + m) * 255.0F);
        return new Color(r, g, b, alpha);
    }

    /// <summary>
    /// Converts the <see cref="Color"/> to a <see cref="SKColor"/>
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The <see cref="SKColor"/> representation of the <see cref="Color"/>.</returns>
    public static implicit operator SKColor(Color color) => new(color.Red, color.Green, color.Blue, color.Alpha);

    /// <summary>
    /// Converts the <see cref="SKColor"/> to a <see cref="Color"/>
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The <see cref="Color"/> representation of the <see cref="SKColor"/>.</returns>
    public static implicit operator Color(SKColor color) => new(color.Red, color.Green, color.Blue, color.Alpha);


    /// <inheritdoc />
    public static Color Parse(string s, IFormatProvider? provider) => Parse(s.AsSpan(), provider);

    /// <inheritdoc />
    public static bool TryParse(string? s, IFormatProvider? provider, out Color result) =>
        TryParse(s.AsSpan(), provider, out result);

    /// <inheritdoc />
    public static Color Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if (s.Length > 0 && s[0] is '#')
            s = s[1..];
        if (uint.TryParse(s, provider, out var value))
            return new Color(value);
        var str = s.ToString().ToLowerInvariant();
        return str switch
        {
            "black"       => Colors.Black,
            "white"       => Colors.White,
            "red"         => Colors.Red,
            "green"       => Colors.Green,
            "blue"        => Colors.Blue,
            "yellow"      => Colors.Yellow,
            "cyan"        => Colors.Cyan,
            "magenta"     => Colors.Magenta,
            "transparent" => Colors.Transparent,
            "lightgray"   => Colors.LightGray,
            "darkgray"    => Colors.DarkGray,
            "gray"        => Colors.Gray,
            "orange"      => Colors.Orange,
            "brown"       => Colors.Brown,
            "pink"        => Colors.Pink,
            "purple"      => Colors.Purple,
            _             => throw new FormatException($"The string '{s}' is not a valid color.")
        };
    }

    /// <inheritdoc />
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Color result)
    {
        if (s.Length > 0 && s[0] is '#')
            s = s[1..];
        if (uint.TryParse(s, provider, out var value))
        {
            result = new Color(value);
            return true;
        }

        var str = s.ToString().ToLowerInvariant();
        var tmp = str switch
        {
            "black"       => Colors.Black,
            "white"       => Colors.White,
            "red"         => Colors.Red,
            "green"       => Colors.Green,
            "blue"        => Colors.Blue,
            "yellow"      => Colors.Yellow,
            "cyan"        => Colors.Cyan,
            "magenta"     => Colors.Magenta,
            "transparent" => Colors.Transparent,
            "lightgray"   => Colors.LightGray,
            "darkgray"    => Colors.DarkGray,
            "gray"        => Colors.Gray,
            "orange"      => Colors.Orange,
            "brown"       => Colors.Brown,
            "pink"        => Colors.Pink,
            "purple"      => Colors.Purple,
            _             => default(Color?),
        };
        if (tmp is not null)
        {
            result = tmp.Value;
            return true;
        }

        result = default;
        return false;
    }

    /// <inheritdoc />
    public override string ToString() => $"{nameof(Color)} {{ #{Red:X2}{Green:X2}{Blue:X2}{Alpha:X2} }}";
}