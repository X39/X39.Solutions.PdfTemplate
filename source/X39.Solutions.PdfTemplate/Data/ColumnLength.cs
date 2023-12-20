using System.ComponentModel;
using System.Globalization;

namespace X39.Solutions.PdfTemplate.Data;

/// <summary>
/// Defines a size with a value and a <see cref="EColumnUnit"/>
/// </summary>
[TypeConverter(typeof(ColumnLengthConverter))]
[PublicAPI]
public readonly record struct ColumnLength : ISpanParsable<ColumnLength>
{
    /// <summary>
    /// Creates a new <see cref="ColumnLength"/> which will fit the available space.
    /// </summary>
    public ColumnLength() : this(new Length(default, ELengthUnit.Auto))
    {
    }

    /// <summary>
    /// Creates a new <see cref="ColumnLength"/> with the given parts value.
    /// </summary>
    /// <param name="value">The value of the size</param>
    public ColumnLength(float value)
    {
        Value = value;
        Unit  = EColumnUnit.Parts;
    }

    /// <summary>
    /// Creates a new <see cref="ColumnLength"/> with the given length
    /// </summary>
    /// <param name="length">The value of the size</param>
    public ColumnLength(Length length)
    {
        Length = length;
        Unit  = EColumnUnit.Lenght;
    }

    /// <summary>
    /// The size mode, indicating how the <see cref="Value"/> should be interpreted.
    /// </summary>
    public EColumnUnit Unit { get; init; }

    /// <summary>
    /// The value of the size.
    /// </summary>
    /// <remarks>
    /// Either this or <see cref="Length"/> must be set.
    /// </remarks>
    public float? Value { get; init; }
    
    /// <summary>
    /// The value of the size.
    /// </summary>
    /// <remarks>
    /// Either this or <see cref="Value"/> must be set.
    /// </remarks>
    public Length? Length { get; init; }

    /// <inheritdoc />
    public static ColumnLength Parse(string s, IFormatProvider? provider) => Parse(s.AsSpan(), provider);

    /// <inheritdoc />
    public static bool TryParse(string? s, IFormatProvider? provider, out ColumnLength result) =>
        TryParse(s.AsSpan(), provider, out result);


    /// <inheritdoc />
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out ColumnLength result)
    {
        s = s.Trim();
        if (s.IsEmpty)
        {
            result = default;
            return false;
        }

        var endOfNumber = 0;
        var dotFound = false;
        // Find end of number
        for (var i = 0; i < s.Length; i++)
        {
            if (s[i].IsDigit())
                continue;
            if (s[i] == '.')
            {
                if (dotFound)
                {
                    result = default;
                    return false;
                }

                dotFound = true;
                continue;
            }

            endOfNumber = i;
            break;
        }

        var number = s[..endOfNumber];
        var unit = s[endOfNumber..].Trim();
        if (unit.IsEmpty)
        {
            result = default;
            return false;
        }

        switch (unit)
        {
            case "*":
                if (number.IsEmpty)
                {
                    result = default;
                    return false;
                }

                var value = float.Parse(number, CultureInfo.InvariantCulture);
                result = new ColumnLength(value);
                return true;
            default:
                if (!Data.Length.TryParse(s, provider, out var length))
                {
                    result = default;
                    return false;
                }
                result = new ColumnLength(length);
                return true;
        }
    }

    /// <inheritdoc />
    public static ColumnLength Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if (!TryParse(s, provider, out var result))
            throw new FormatException($"The given string '{s.ToString()}' is not a valid {nameof(ColumnLength)}");
        return result;
    }

    /// <summary>
    /// Converts the <see cref="ColumnLength"/> to a string.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => ToString(CultureInfo.CurrentCulture);

    /// <summary>
    /// Converts the <see cref="ColumnLength"/> to a string.
    /// </summary>
    /// <param name="provider">The format provider to be used.</param>
    /// <returns></returns>
    public string ToString(IFormatProvider? provider)
    {
        return Unit switch
        {
            EColumnUnit.Parts  => string.Format(provider, "{0}*", Value),
            EColumnUnit.Lenght => Length?.ToString(provider) ?? throw new InvalidOperationException("Length is null"),
            _                  => throw new InvalidEnumArgumentException(nameof(Unit), (int) Unit, typeof(EColumnUnit)),
        };
    }
}