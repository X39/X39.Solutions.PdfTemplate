using System.ComponentModel;
using System.Globalization;

namespace X39.Solutions.PdfTemplate.Data;

/// <summary>
/// Describes a thickness with a size for each side.
/// </summary>
/// <param name="Left">The size of the left side</param>
/// <param name="Top">The size of the top side</param>
/// <param name="Right">The size of the right side</param>
/// <param name="Bottom">The size of the bottom side</param>
[TypeConverter(typeof(ThicknessConverter))]
public readonly record struct Thickness(Length Left, Length Top, Length Right, Length Bottom) : ISpanParsable<Thickness>
{
    /// <summary>
    /// Creates a new instance of <see cref="Thickness"/>.
    /// </summary>
    /// <param name="all">The length of all sides</param>
    public Thickness(Length all) : this(all, all, all, all)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="Thickness"/>.
    /// </summary>
    /// <param name="horizontal">The length of the horizontal sides</param>
    /// <param name="vertical">The length of the vertical sides</param>
    public Thickness(Length horizontal, Length vertical) : this(horizontal, vertical, horizontal, vertical)
    {
    }

    /// <summary>
    /// Translates the thickness to a pixels rectangle.
    /// </summary>
    /// <param name="bounds">The bounds of the rectangle</param>
    /// <returns>The translated rectangle</returns>
    public Rectangle ToRectangle(Rectangle bounds)
    {
        var left = Left.ToPixels(bounds.Width);
        var top = Top.ToPixels(bounds.Height);
        var right = Right.ToPixels(bounds.Width);
        var bottom = Bottom.ToPixels(bounds.Height);
        return new Rectangle(
            left,
            top,
            right - left,
            bottom - top);
    }

    /// <summary>
    /// Translates the thickness to a pixels rectangle.
    /// </summary>
    /// <param name="bounds">The bounds of the rectangle</param>
    /// <returns>The translated rectangle</returns>
    public Rectangle ToRectangle(Size bounds)
    {
        var left = Left.ToPixels(bounds.Width);
        var top = Top.ToPixels(bounds.Height);
        var width = Right.ToPixels(bounds.Width);
        var height = Bottom.ToPixels(bounds.Height);
        return new Rectangle(
            left,
            top,
            width,
            height);
    }

    /// <inheritdoc />
    public static Thickness Parse(string s, IFormatProvider? provider) => Parse(s.AsSpan(), provider);

    /// <inheritdoc />
    public static bool TryParse(string? s, IFormatProvider? provider, out Thickness result) =>
        TryParse(s.AsSpan(), provider, out result);

    /// <inheritdoc />
    public static Thickness Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        var partCount = 1;
        var wasSeparator = false;
        foreach (var t in s)
        {
            if (t is ' ')
            {
                if (!wasSeparator)
                    partCount++;
                wasSeparator = true;
            }
            else
            {
                wasSeparator = false;
            }
        }

        if (partCount is not 1 and not 2 and not 4)
            throw new FormatException($"The thickness '{s}' is not in the correct format.");

        Length left;
        Length top;
        Length right;
        Length bottom;
        switch (partCount)
        {
            case 1:
                left   = Length.Parse(s, provider);
                top    = left;
                right  = left;
                bottom = left;
                break;
            case 2:
                left   = Length.Parse(s[..s.IndexOf(' ')], provider);
                top    = Length.Parse(s[(s.LastIndexOf(' ') + 1)..], provider);
                right  = left;
                bottom = top;
                break;
            case 4:
                left   = Length.Parse(s[..s.IndexOf(' ')], provider);
                s = s[(s.IndexOf(' ') + 1)..];
                top    = Length.Parse(s[..s.IndexOf(' ')], provider);
                s = s[(s.IndexOf(' ') + 1)..];
                right  = Length.Parse(s[..s.IndexOf(' ')], provider);
                bottom = Length.Parse(s[(s.IndexOf(' ') + 1)..], provider);
                break;
            default:
                throw new FormatException($"The thickness '{s}' is not in the correct format.");
        }
        return new Thickness(left, top, right, bottom);
    }

    /// <inheritdoc />
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Thickness result)
    {
        var partCount = 1;
        var wasSeparator = false;
        foreach (var t in s)
        {
            if (t is ' ')
            {
                if (!wasSeparator)
                    partCount++;
                wasSeparator = true;
            }
            else
            {
                wasSeparator = false;
            }
        }

        if (partCount is not 1 and not 2 and not 4)
        {
            result = default;
            return false;
        }
#pragma warning disable CA2201

        var left = partCount switch
        {
            1 => Length.Parse(s, provider),
            2 => Length.Parse(s[..s.IndexOf(' ')], provider),
            4 => Length.Parse(s[..s.IndexOf(' ')], provider),
            _ => throw new Exception(
                "Impossible exception as it is checked before. If this is ever thrown, something is seriously wrong."),
        };
        var top = partCount switch
        {
            1 => left,
            2 => Length.Parse(s[(s.IndexOf(' ') + 1)..], provider),
            4 => Length.Parse(s[(s.IndexOf(' ') + 1)..], provider),
            _ => throw new Exception(
                "Impossible exception as it is checked before. If this is ever thrown, something is seriously wrong."),
        };
        var right = partCount switch
        {
            1 => left,
            2 => left,
            4 => Length.Parse(s[(s.LastIndexOf(' ') + 1)..], provider),
            _ => throw new Exception(
                "Impossible exception as it is checked before. If this is ever thrown, something is seriously wrong."),
        };
        var bottom = partCount switch
        {
            1 => left,
            2 => top,
            4 => Length.Parse(s[(s.LastIndexOf(' ') + 1)..], provider),
            _ => throw new Exception(
                "Impossible exception as it is checked before. If this is ever thrown, something is seriously wrong."),
        };
#pragma warning restore CA2201
        result = new Thickness(left, top, right, bottom);
        return true;
    }

    /// <inheritdoc />
    public override string ToString() => ToString(CultureInfo.CurrentCulture);

    /// <inheritdoc cref="ToString()"/>
    public string ToString(IFormatProvider? provider)
    {
        if (Left == Top && Left == Right && Left == Bottom)
            return Left.ToString(provider);
        if (Left == Right && Top == Bottom)
            return $"{Left.ToString(provider)} {Top.ToString(provider)}";
        return
            $"{Left.ToString(provider)} {Top.ToString(provider)} {Right.ToString(provider)} {Bottom.ToString(provider)}";
    }
}