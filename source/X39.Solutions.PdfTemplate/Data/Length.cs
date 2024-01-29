using System.ComponentModel;
using System.Globalization;

namespace X39.Solutions.PdfTemplate.Data;

/// <summary>
/// Defines a size with a value and a <see cref="ELengthUnit"/>
/// </summary>
[TypeConverter(typeof(LengthConverter))]
[PublicAPI]
public readonly record struct Length : ISpanParsable<Length>
{
    /// <summary>
    /// Creates a new <see cref="Length"/> which will fit the available space.
    /// </summary>
    public Length() : this(default, ELengthUnit.Auto)
    {
    }

    /// <summary>
    /// Defines a size with a value and a <see cref="ELengthUnit"/>
    /// </summary>
    /// <param name="value">The value of the size</param>
    /// <param name="unit">The size mode, indicating how the value should be interpreted</param>
    public Length(float value, ELengthUnit unit)
    {
        Value = value;
        Unit  = unit;
    }

    /// <summary>The value of the size</summary>
    public float Value { get; init; }

    /// <summary>The size unit, indicating how the value should be interpreted</summary>
    public ELengthUnit Unit { get; init; }

    /// <summary>
    /// Deconstructs the <see cref="Length"/> into a <see cref="float"/> and a <see cref="ELengthUnit"/>
    /// </summary>
    /// <param name="value">The value of the size</param>
    /// <param name="lengthUnit">The unit of the size</param>
    public void Deconstruct(out float value, out ELengthUnit lengthUnit)
    {
        value      = Value;
        lengthUnit = Unit;
    }

    /// <summary>
    /// Implicitly converts a <see cref="float"/> into a <see cref="Length"/> with <see cref="ELengthUnit.Pixel"/>
    /// </summary>
    /// <param name="valuePx">The value of the size</param>
    /// <returns>A new <see cref="Length"/> with <see cref="ELengthUnit.Pixel"/></returns>
    public static implicit operator Length(float valuePx) => new(valuePx, ELengthUnit.Pixel);

    /// <summary>
    /// Translates the <see cref="Length"/> into a <see cref="float"/> based on the given bounds and <see cref="Unit"/>.
    /// </summary>
    /// <param name="boundsPx">The bounds to use for the calculation in case of <see cref="ELengthUnit.Percent"/> in pixels</param>
    /// <param name="dpi">The DPI to use for the calculation in case of <see cref="ELengthUnit.Points"/> in pixels</param>
    /// <returns>The pixel value of the <see cref="Length"/></returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <see cref="Unit"/> is not a valid <see cref="ELengthUnit"/></exception>
    public float ToPixels(float boundsPx, float dpi)
    {
        return Unit switch
        {
            ELengthUnit.Pixel => Value,
            ELengthUnit.Percent => Value * boundsPx,
            ELengthUnit.Points => Value * dpi / 72.272F,
            ELengthUnit.Auto => boundsPx,
            ELengthUnit.Millimeters => Value * dpi * 0.0393701F,
            ELengthUnit.Centimeters => Value * dpi * 0.393701F,
            ELengthUnit.Inches => Value * dpi,
            _ => throw new InvalidEnumArgumentException(nameof(Unit), (int) Unit, typeof(ELengthUnit)),
        };
    }

    /// <inheritdoc />
    public static Length Parse(string s, IFormatProvider? provider) => Parse(s.AsSpan(), provider);

    /// <inheritdoc />
    public static bool TryParse(string? s, IFormatProvider? provider, out Length result) =>
        TryParse(s.AsSpan(), provider, out result);

    /// <inheritdoc />
    public static Length Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if (!TryParse(s, provider, out var result))
            throw new FormatException($"The given string '{s.ToString()}' is not a valid {nameof(Length)}.");
        return result;
    }

    /// <inheritdoc />
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Length result)
    {
        var unitOffset = 0;
        for (var i = s.Length - 1; i >= 0 && !s[i].IsDigit() && s[i] is not '.'; i--)
            unitOffset++;
        var unit = s[^unitOffset..];
        var number = s[..^unitOffset];
        ELengthUnit sizeUnit;
        switch (unit)
        {
            case "":
            case "px":
            case "pX":
            case "Px":
            case "PX":
                sizeUnit = ELengthUnit.Pixel;
                break;
            case "%":
                sizeUnit = ELengthUnit.Percent;
                break;
            case "pt":
            case "pT":
            case "Pt":
            case "PT":
                sizeUnit = ELengthUnit.Points;
                break;
            case "auto":
            case "autO":
            case "auTo":
            case "auTO":
            case "aUto":
            case "aUtO":
            case "aUTo":
            case "aUTO":
            case "Auto":
            case "AutO":
            case "AuTo":
            case "AuTO":
            case "AUto":
            case "AUtO":
            case "AUTo":
            case "AUTO":
                sizeUnit = ELengthUnit.Auto;
                break;
            case "in":
            case "iN":
            case "In":
            case "IN":
                sizeUnit = ELengthUnit.Inches;
                break;
            case "mm":
            case "mM":
            case "Mm":
            case "MM":
                sizeUnit = ELengthUnit.Millimeters;
                break;
            case "cm":
            case "cM":
            case "Cm":
            case "CM":
                sizeUnit = ELengthUnit.Centimeters;
                break;
            default:
                result = default;
                return false;
        }

        if (sizeUnit is ELengthUnit.Auto)
        {
            result = new Length(default, ELengthUnit.Auto);
            return true;
        }

        if (!float.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out var sizeValue))
        {
            result = default;
            return false;
        }

        if (sizeUnit is ELengthUnit.Percent)
            sizeValue /= 100.0F;
        result = new Length(sizeValue, sizeUnit);
        return true;
    }

    /// <inheritdoc />
    public override string ToString() => ToString(CultureInfo.CurrentCulture);

    /// <inheritdoc cref="ToString()"/>
    public string ToString(IFormatProvider? provider)
    {
        var sizeValue = Value;
        var sizeMode = Unit;
        if (sizeMode is ELengthUnit.Percent)
            sizeValue *= 100.0F;
        var sizeUnit = sizeMode switch
        {
            ELengthUnit.Pixel       => "px",
            ELengthUnit.Percent     => "%",
            ELengthUnit.Points      => "pt",
            ELengthUnit.Auto        => "auto",
            ELengthUnit.Millimeters => "mm",
            ELengthUnit.Centimeters => "cm",
            ELengthUnit.Inches      => "in",
            _                       => throw new NotSupportedException($"The size mode '{sizeMode}' is not supported."),
        };
        return sizeMode is ELengthUnit.Auto
            ? sizeUnit
            : string.Concat(sizeValue.ToString(CultureInfo.InvariantCulture), sizeUnit);
    }
    
    /// <summary>
    /// Divides the <see cref="Length"/> by the given <paramref name="right"/> value.
    /// </summary>
    public static Length operator /(Length left, float right)
    {
        return left.Unit is ELengthUnit.Auto ? left : left with { Value = left.Value / right };
    }
    
    /// <summary>
    /// Multiplies the <see cref="Length"/> by the given <paramref name="right"/> value.
    /// </summary>
    public static Length operator *(Length left, float right)
    {
        return left.Unit is ELengthUnit.Auto ? left : left with { Value = left.Value * right };
    }

    /// <summary>
    /// Compares whether the <paramref name="left"/> <see cref="Length"/> is smaller than the <paramref name="right"/> <see cref="Length"/>.
    /// </summary>
    public static bool operator <(Length left, Length right)
    {
        return left.ToPixels(100, 100) < right.ToPixels(100, 100);
    }

    /// <summary>
    /// Compares whether the <paramref name="left"/> <see cref="Length"/> is greater than the <paramref name="right"/> <see cref="Length"/>.
    /// </summary>
    public static bool operator >(Length left, Length right)
    {
        return left.ToPixels(100, 100) > right.ToPixels(100, 100);
    }
}