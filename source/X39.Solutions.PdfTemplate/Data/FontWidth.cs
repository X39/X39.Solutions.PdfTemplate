using System.Globalization;
using System.Numerics;

namespace X39.Solutions.PdfTemplate.Data;

/// <summary>
/// Wrapper for <see cref="ushort"/> to represent a font Width.
/// </summary>
/// <param name="Value">The value of the font Width.</param>
public readonly record struct FontWidth(ushort Value) : INumber<FontWidth>
{
    /// <summary>
    /// Converts a <see cref="FontWidth"/> to a <see cref="ushort"/>.
    /// </summary>
    /// <param name="value">The <see cref="FontWidth"/> to convert.</param>
    /// <returns>The <see cref="ushort"/> representation of the <see cref="FontWidth"/>.</returns>
    public static implicit operator ushort(FontWidth value) => value.Value;

    /// <summary>
    /// Converts a <see cref="ushort"/> to a <see cref="FontWidth"/>.
    /// </summary>
    /// <param name="value">The <see cref="ushort"/> to convert.</param>
    /// <returns>The <see cref="FontWidth"/> representation of the <see cref="ushort"/>.</returns>
    public static implicit operator FontWidth(ushort value) => new(value);

    /// <inheritdoc />
    public int CompareTo(object? obj)
    {
        return Value.CompareTo(obj);
    }

    /// <inheritdoc />
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return Value.ToString(format, formatProvider);
    }

    /// <inheritdoc />
    public bool TryFormat(
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        return Value.TryFormat(destination, out charsWritten, format, provider);
    }

    /// <inheritdoc />
    public int CompareTo(FontWidth other) => Value.CompareTo(other.Value);

    /// <inheritdoc />
    public static FontWidth Parse(string s, IFormatProvider? provider) => new FontWidth(ushort.Parse(s, provider));

    /// <inheritdoc />
    public static bool TryParse(string? s, IFormatProvider? provider, out FontWidth result)
    {
        if (ushort.TryParse(s, provider, out var value))
        {
            result = new FontWidth(value);
            return true;
        }

        result = default;
        return false;
    }

    /// <inheritdoc />
    public static FontWidth Parse(ReadOnlySpan<char> s, IFormatProvider? provider) =>
        new FontWidth(ushort.Parse(s, provider));

    /// <inheritdoc />
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out FontWidth result)
    {
        if (ushort.TryParse(s, provider, out var value))
        {
            result = new FontWidth(value);
            return true;
        }

        result = default;
        return false;
    }

    /// <inheritdoc />
    public static FontWidth operator +(FontWidth left, FontWidth right) =>
        new FontWidth((ushort) (left.Value + right.Value));

    /// <inheritdoc />
    public static FontWidth AdditiveIdentity => new FontWidth(0);

    /// <inheritdoc />
    public static bool operator >(FontWidth left, FontWidth right) => left.Value > right.Value;

    /// <inheritdoc />
    public static bool operator >=(FontWidth left, FontWidth right) => left.Value >= right.Value;

    /// <inheritdoc />
    public static bool operator <(FontWidth left, FontWidth right) => left.Value < right.Value;

    /// <inheritdoc />
    public static bool operator <=(FontWidth left, FontWidth right) => left.Value <= right.Value;

    /// <inheritdoc />
    public static FontWidth operator --(FontWidth value) => new FontWidth((ushort) (value.Value - 1));

    /// <inheritdoc />
    public static FontWidth operator /(FontWidth left, FontWidth right) =>
        new FontWidth((ushort) (left.Value / right.Value));

    /// <inheritdoc />
    public static FontWidth operator ++(FontWidth value) => new FontWidth((ushort) (value.Value + 1));

    /// <inheritdoc />
    public static FontWidth operator %(FontWidth left, FontWidth right) =>
        new FontWidth((ushort) (left.Value % right.Value));

    /// <inheritdoc />
    public static FontWidth MultiplicativeIdentity => new FontWidth(1);

    /// <inheritdoc />
    public static FontWidth operator *(FontWidth left, FontWidth right) =>
        new FontWidth((ushort) (left.Value * right.Value));

    /// <inheritdoc />
    public static FontWidth operator -(FontWidth left, FontWidth right) =>
        new FontWidth((ushort) (left.Value - right.Value));

    /// <inheritdoc />
    public static FontWidth operator -(FontWidth value) => new FontWidth((ushort) (-value.Value));

    /// <inheritdoc />
    public static FontWidth operator +(FontWidth value) => new FontWidth((ushort) (+value.Value));

    /// <inheritdoc />
    public static FontWidth Abs(FontWidth value) => new FontWidth((ushort) Math.Abs(value.Value));

    /// <inheritdoc />
    public static bool IsCanonical(FontWidth value) => true;

    /// <inheritdoc />
    public static bool IsComplexNumber(FontWidth value) => false;

    /// <inheritdoc />
    public static bool IsEvenInteger(FontWidth value) => value.Value % 2 == 0;

    /// <inheritdoc />
    public static bool IsFinite(FontWidth value) => true;

    /// <inheritdoc />
    public static bool IsImaginaryNumber(FontWidth value) => false;

    /// <inheritdoc />
    public static bool IsInfinity(FontWidth value) => false;

    /// <inheritdoc />
    public static bool IsInteger(FontWidth value) => true;

    /// <inheritdoc />
    public static bool IsNaN(FontWidth value) => false;

    /// <inheritdoc />
    public static bool IsNegative(FontWidth value) => false;

    /// <inheritdoc />
    public static bool IsNegativeInfinity(FontWidth value) => false;

    /// <inheritdoc />
    public static bool IsNormal(FontWidth value) => true;

    /// <inheritdoc />
    public static bool IsOddInteger(FontWidth value) => value.Value % 2 == 1;

    /// <inheritdoc />
    public static bool IsPositive(FontWidth value) => true;

    /// <inheritdoc />
    public static bool IsPositiveInfinity(FontWidth value) => false;

    /// <inheritdoc />
    public static bool IsRealNumber(FontWidth value) => true;

    /// <inheritdoc />
    public static bool IsSubnormal(FontWidth value) => false;

    /// <inheritdoc />
    public static bool IsZero(FontWidth value) => value.Value == 0;

    /// <inheritdoc />
    public static FontWidth MaxMagnitude(FontWidth x, FontWidth y) => x.Value > y.Value ? x : y;

    /// <inheritdoc />
    public static FontWidth MaxMagnitudeNumber(FontWidth x, FontWidth y) => x.Value > y.Value ? x : y;

    /// <inheritdoc />
    public static FontWidth MinMagnitude(FontWidth x, FontWidth y) => x.Value < y.Value ? x : y;

    /// <inheritdoc />
    public static FontWidth MinMagnitudeNumber(FontWidth x, FontWidth y) => x.Value < y.Value ? x : y;

    /// <inheritdoc />
    public static FontWidth Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) =>
        new FontWidth(ushort.Parse(s, style, provider));

    /// <inheritdoc />
    public static FontWidth Parse(string s, NumberStyles style, IFormatProvider? provider) =>
        new FontWidth(ushort.Parse(s, style, provider));

    /// <inheritdoc />
    public static bool TryConvertFromChecked<TOther>(TOther value, out FontWidth result)
        where TOther : INumberBase<TOther>
    {
        if (TOther.TryConvertToChecked<ushort>(value, out var converted))
        {
            result = new FontWidth(converted);
            return true;
        }

        result = default;
        return false;
    }

    /// <inheritdoc />
    public static bool TryConvertFromSaturating<TOther>(TOther value, out FontWidth result)
        where TOther : INumberBase<TOther>
    {
        if (TOther.TryConvertToSaturating<ushort>(value, out var converted))
        {
            result = new FontWidth(converted);
            return true;
        }

        result = default;
        return false;
    }

    /// <inheritdoc />
    public static bool TryConvertFromTruncating<TOther>(TOther value, out FontWidth result)
        where TOther : INumberBase<TOther>
    {
        if (TOther.TryConvertToTruncating<ushort>(value, out var converted))
        {
            result = new FontWidth(converted);
            return true;
        }

        result = default;
        return false;
    }

    /// <inheritdoc />
    public static bool TryConvertToChecked<TOther>(FontWidth value, out TOther result)
        where TOther : INumberBase<TOther>
    {
        if (TOther.TryConvertFromChecked(value.Value, out var converted))
        {
            result = converted;
            return true;
        }

        result = default!;
        return false;
    }

    /// <inheritdoc />
    public static bool TryConvertToSaturating<TOther>(FontWidth value, out TOther result)
        where TOther : INumberBase<TOther>
    {
        if (TOther.TryConvertFromSaturating(value.Value, out var converted))
        {
            result = converted;
            return true;
        }

        result = default;
        return false;
    }

    /// <inheritdoc />
    public static bool TryConvertToTruncating<TOther>(FontWidth value, out TOther result)
        where TOther : INumberBase<TOther>
    {
        if (TOther.TryConvertFromTruncating(value.Value, out var converted))
        {
            result = converted;
            return true;
        }

        result = default!;
        return false;
    }

    /// <inheritdoc />
    public static bool TryParse(
        ReadOnlySpan<char> s,
        NumberStyles style,
        IFormatProvider? provider,
        out FontWidth result)
    {
        if (ushort.TryParse(s, style, provider, out var parsed))
        {
            result = new FontWidth(parsed);
            return true;
        }

        result = default;
        return false;
    }

    /// <inheritdoc />
    public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out FontWidth result)
    {
        if (ushort.TryParse(s, style, provider, out var parsed))
        {
            result = new FontWidth(parsed);
            return true;
        }

        result = default;
        return false;
    }

    /// <inheritdoc />
    public static FontWidth One => new(1);

    /// <inheritdoc />
    public static int Radix => 10;

    /// <inheritdoc />
    public static FontWidth Zero => default;
}