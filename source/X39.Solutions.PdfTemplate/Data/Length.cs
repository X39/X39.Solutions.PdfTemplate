using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace X39.Solutions.PdfTemplate.Data;

/// <summary>
/// Defines a size with a value and a <see cref="ELengthMode"/>
/// </summary>
[TypeConverter(typeof(LengthConverter))]
[PublicAPI]
public readonly record struct Length : ISpanParsable<Length>
{
    /// <summary>
    /// Creates a new <see cref="Length"/> which will fit the available space.
    /// </summary>
    public Length() : this(1.0F, ELengthMode.Percent)
    {
    }

    /// <summary>
    /// Defines a size with a value and a <see cref="ELengthMode"/>
    /// </summary>
    /// <param name="value">The value of the size</param>
    /// <param name="lengthMode">The size mode, indicating how the value should be interpreted</param>
    public Length(float value, ELengthMode lengthMode)
    {
        Value      = value;
        LengthMode = lengthMode;
    }

    /// <summary>The value of the size</summary>
    public float Value { get; init; }

    /// <summary>The size mode, indicating how the value should be interpreted</summary>
    public ELengthMode LengthMode { get; init; }

    /// <summary>
    /// Deconstructs the <see cref="Length"/> into a <see cref="float"/> and a <see cref="ELengthMode"/>
    /// </summary>
    /// <param name="value">The value of the size</param>
    /// <param name="lengthMode">The mode of the size</param>
    public void Deconstruct(out float value, out ELengthMode lengthMode)
    {
        value    = Value;
        lengthMode = LengthMode;
    }
    
    /// <summary>
    /// Implicitly converts a <see cref="float"/> into a <see cref="Length"/> with <see cref="ELengthMode.Pixel"/>
    /// </summary>
    /// <param name="value">The value of the size</param>
    /// <returns>A new <see cref="Length"/> with <see cref="ELengthMode.Pixel"/></returns>
    public static implicit operator Length(float value) => new(value, ELengthMode.Pixel);

    /// <summary>
    /// Translates the <see cref="Length"/> into a <see cref="float"/> based on the given bounds and <see cref="LengthMode"/>.
    /// </summary>
    /// <param name="bounds">The bounds to use for the calculation in case of <see cref="ELengthMode.Percent"/> in pixels</param>
    /// <returns>The pixel value of the <see cref="Length"/></returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <see cref="LengthMode"/> is not a valid <see cref="ELengthMode"/></exception>
    public float ToPixels(float bounds)
    {
        return LengthMode switch
        {
            ELengthMode.Pixel => Value,
            ELengthMode.Percent => Value * bounds,
            _ => throw new InvalidEnumArgumentException(nameof(LengthMode), (int)LengthMode, typeof(ELengthMode)),
        };
    }

    /// <inheritdoc />
    public static Length Parse(string s, IFormatProvider? provider) => Parse(s.AsSpan(), provider);

    /// <inheritdoc />
    public static bool TryParse(string? s, IFormatProvider? provider, out Length result) => TryParse(s.AsSpan(), provider, out result);

    /// <inheritdoc />
    public static Length Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        var unitLength = 0;
        for (var i = s.Length - 1; i >= 0 && !s[i].IsDigit() && s[i] is not '.'; i--)
            unitLength++;
        var unit = s[^unitLength..];
        var number = s[..^unitLength];
        var sizeValue = float.Parse(
            number,
            NumberStyles.Float | NumberStyles.Number,
            provider);
        var sizeMode = unit switch
        {
            ""   => ELengthMode.Pixel,
            "px" => ELengthMode.Pixel,
            "%"  => ELengthMode.Percent,
            _    => throw new NotSupportedException($"The unit '{unit}' is not supported.")
        };
        if (sizeMode is ELengthMode.Percent)
            sizeValue /= 100.0F;
        return new Length(sizeValue, sizeMode);
    }

    /// <inheritdoc />
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Length result)
    {
        var unitLength = 0;
        for (var i = s.Length - 1; i >= 0 && s[i].IsLetter(); i--)
            unitLength++;
        var unit = s[^unitLength..];
        var number = s[..^unitLength];
        var sizeValue = float.Parse(
            number,
            NumberStyles.Float | NumberStyles.Number,
            provider);
        var sizeMode = unit switch
        {
            ""   => ELengthMode.Pixel,
            "px" => ELengthMode.Pixel,
            "%"  => ELengthMode.Percent,
            _    => default(ELengthMode?),
        };
        if (sizeMode is null)
        {
            result = default;
            return false;
        }
        if (sizeMode is ELengthMode.Percent)
            sizeValue /= 100.0F;
        result = new Length(sizeValue, sizeMode.Value);
        return true;
    }

    /// <inheritdoc />
    public override string ToString() => ToString(CultureInfo.CurrentCulture);
    /// <inheritdoc cref="ToString()"/>
    public string ToString(IFormatProvider? provider)
    {
        var sizeValue = Value;
        var sizeMode = LengthMode;
        if (sizeMode is ELengthMode.Percent)
            sizeValue *= 100.0F;
        var sizeUnit = sizeMode switch
        {
            ELengthMode.Pixel   => "px",
            ELengthMode.Percent => "%",
            _                   => throw new NotSupportedException($"The size mode '{sizeMode}' is not supported."),
        };
        return string.Concat(sizeValue.ToString(provider), sizeUnit);
    }
}