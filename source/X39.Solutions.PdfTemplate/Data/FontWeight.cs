using System.Globalization;
using System.Numerics;

namespace X39.Solutions.PdfTemplate.Data;

/// <summary>
/// Wrapper for <see cref="ushort"/> to represent a font weight.
/// </summary>
/// <param name="Value">The value of the font weight.</param>
public readonly record struct FontWeight(ushort Value) : INumber<FontWeight>
{
    /// <summary>
    /// Converts a <see cref="FontWeight"/> to a <see cref="ushort"/>.
    /// </summary>
    /// <param name="value">The <see cref="FontWeight"/> to convert.</param>
    /// <returns>The <see cref="ushort"/> representation of the <see cref="FontWeight"/>.</returns>
    public static implicit operator ushort(FontWeight value) => value.Value;

    /// <summary>
    /// Converts a <see cref="ushort"/> to a <see cref="FontWeight"/>.
    /// </summary>
    /// <param name="value">The <see cref="ushort"/> to convert.</param>
    /// <returns>The <see cref="FontWeight"/> representation of the <see cref="ushort"/>.</returns>
    public static implicit operator FontWeight(ushort value) => new(value);

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
    public int CompareTo(FontWeight other) => Value.CompareTo(other.Value);

    /// <inheritdoc />
    public static FontWeight Parse(string s, IFormatProvider? provider) => new FontWeight(ushort.Parse(s, provider));

    /// <inheritdoc />
    public static bool TryParse(string? s, IFormatProvider? provider, out FontWeight result)
    {
        if (ushort.TryParse(s, provider, out var value))
        {
            result = new FontWeight(value);
            return true;
        }

        result = default;
        return false;
    }

    /// <inheritdoc />
    public static FontWeight Parse(ReadOnlySpan<char> s, IFormatProvider? provider) =>
        new FontWeight(ushort.Parse(s, provider));

    /// <inheritdoc />
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out FontWeight result)
    {
        if (ushort.TryParse(s, provider, out var value))
        {
            result = new FontWeight(value);
            return true;
        }

        result = default;
        return false;
    }

    /// <inheritdoc />
    public static FontWeight operator +(FontWeight left, FontWeight right) =>
        new FontWeight((ushort) (left.Value + right.Value));

    /// <inheritdoc />
    public static FontWeight AdditiveIdentity => new FontWeight(0);

    /// <inheritdoc />
    public static bool operator >(FontWeight left, FontWeight right) => left.Value > right.Value;

    /// <inheritdoc />
    public static bool operator >=(FontWeight left, FontWeight right) => left.Value >= right.Value;

    /// <inheritdoc />
    public static bool operator <(FontWeight left, FontWeight right) => left.Value < right.Value;

    /// <inheritdoc />
    public static bool operator <=(FontWeight left, FontWeight right) => left.Value <= right.Value;

    /// <inheritdoc />
    public static FontWeight operator --(FontWeight value) => new FontWeight((ushort) (value.Value - 1));

    /// <inheritdoc />
    public static FontWeight operator /(FontWeight left, FontWeight right) =>
        new FontWeight((ushort) (left.Value / right.Value));

    /// <inheritdoc />
    public static FontWeight operator ++(FontWeight value) => new FontWeight((ushort) (value.Value + 1));

    /// <inheritdoc />
    public static FontWeight operator %(FontWeight left, FontWeight right) =>
        new FontWeight((ushort) (left.Value % right.Value));

    /// <inheritdoc />
    public static FontWeight MultiplicativeIdentity => new FontWeight(1);

    /// <inheritdoc />
    public static FontWeight operator *(FontWeight left, FontWeight right) =>
        new FontWeight((ushort) (left.Value * right.Value));

    /// <inheritdoc />
    public static FontWeight operator -(FontWeight left, FontWeight right) =>
        new FontWeight((ushort) (left.Value - right.Value));

    /// <inheritdoc />
    public static FontWeight operator -(FontWeight value) => new FontWeight((ushort) (-value.Value));

    /// <inheritdoc />
    public static FontWeight operator +(FontWeight value) => new FontWeight((ushort) (+value.Value));

    /// <inheritdoc />
    public static FontWeight Abs(FontWeight value) => new FontWeight((ushort) Math.Abs(value.Value));

    /// <inheritdoc />
    public static bool IsCanonical(FontWeight value) => true;

    /// <inheritdoc />
    public static bool IsComplexNumber(FontWeight value) => false;

    /// <inheritdoc />
    public static bool IsEvenInteger(FontWeight value) => value.Value % 2 == 0;

    /// <inheritdoc />
    public static bool IsFinite(FontWeight value) => true;

    /// <inheritdoc />
    public static bool IsImaginaryNumber(FontWeight value) => false;

    /// <inheritdoc />
    public static bool IsInfinity(FontWeight value) => false;

    /// <inheritdoc />
    public static bool IsInteger(FontWeight value) => true;

    /// <inheritdoc />
    public static bool IsNaN(FontWeight value) => false;

    /// <inheritdoc />
    public static bool IsNegative(FontWeight value) => false;

    /// <inheritdoc />
    public static bool IsNegativeInfinity(FontWeight value) => false;

    /// <inheritdoc />
    public static bool IsNormal(FontWeight value) => true;

    /// <inheritdoc />
    public static bool IsOddInteger(FontWeight value) => value.Value % 2 == 1;

    /// <inheritdoc />
    public static bool IsPositive(FontWeight value) => true;

    /// <inheritdoc />
    public static bool IsPositiveInfinity(FontWeight value) => false;

    /// <inheritdoc />
    public static bool IsRealNumber(FontWeight value) => true;

    /// <inheritdoc />
    public static bool IsSubnormal(FontWeight value) => false;

    /// <inheritdoc />
    public static bool IsZero(FontWeight value) => value.Value == 0;

    /// <inheritdoc />
    public static FontWeight MaxMagnitude(FontWeight x, FontWeight y) => x.Value > y.Value ? x : y;

    /// <inheritdoc />
    public static FontWeight MaxMagnitudeNumber(FontWeight x, FontWeight y) => x.Value > y.Value ? x : y;

    /// <inheritdoc />
    public static FontWeight MinMagnitude(FontWeight x, FontWeight y) => x.Value < y.Value ? x : y;

    /// <inheritdoc />
    public static FontWeight MinMagnitudeNumber(FontWeight x, FontWeight y) => x.Value < y.Value ? x : y;

    /// <inheritdoc />
    public static FontWeight Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) =>
        new FontWeight(ushort.Parse(s, style, provider));

    /// <inheritdoc />
    public static FontWeight Parse(string s, NumberStyles style, IFormatProvider? provider) =>
        new FontWeight(ushort.Parse(s, style, provider));

    /// <inheritdoc />
    public static bool TryConvertFromChecked<TOther>(TOther value, out FontWeight result)
        where TOther : INumberBase<TOther>
    {
        if (TOther.TryConvertToChecked<ushort>(value, out var converted))
        {
            result = new FontWeight(converted);
            return true;
        }

        result = default;
        return false;
    }

    /// <inheritdoc />
    public static bool TryConvertFromSaturating<TOther>(TOther value, out FontWeight result)
        where TOther : INumberBase<TOther>
    {
        if (TOther.TryConvertToSaturating<ushort>(value, out var converted))
        {
            result = new FontWeight(converted);
            return true;
        }

        result = default;
        return false;
    }

    /// <inheritdoc />
    public static bool TryConvertFromTruncating<TOther>(TOther value, out FontWeight result)
        where TOther : INumberBase<TOther>
    {
        if (TOther.TryConvertToTruncating<ushort>(value, out var converted))
        {
            result = new FontWeight(converted);
            return true;
        }

        result = default;
        return false;
    }

    /// <inheritdoc />
    public static bool TryConvertToChecked<TOther>(FontWeight value, out TOther result)
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
    public static bool TryConvertToSaturating<TOther>(FontWeight value, out TOther result)
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
    public static bool TryConvertToTruncating<TOther>(FontWeight value, out TOther result)
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
        out FontWeight result)
    {
        if (ushort.TryParse(s, style, provider, out var parsed))
        {
            result = new FontWeight(parsed);
            return true;
        }

        result = default;
        return false;
    }

    /// <inheritdoc />
    public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out FontWeight result)
    {
        if (ushort.TryParse(s, style, provider, out var parsed))
        {
            result = new FontWeight(parsed);
            return true;
        }

        result = default;
        return false;
    }

    /// <inheritdoc />
    public static FontWeight One => new(1);

    /// <inheritdoc />
    public static int Radix => 10;

    /// <inheritdoc />
    public static FontWeight Zero => default;
}