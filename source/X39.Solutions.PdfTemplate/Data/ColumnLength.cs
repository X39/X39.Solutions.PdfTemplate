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
    public ColumnLength() : this(1.0F, EColumnUnit.Auto)
    {
    }

    /// <summary>
    /// Creates a new <see cref="ColumnLength"/> with the given value and <see cref="EColumnUnit"/>
    /// </summary>
    /// <param name="value">The value of the size</param>
    /// <param name="unit">The size mode, indicating how the value should be interpreted</param>
    public ColumnLength(float value, EColumnUnit unit)
    {
        Value = value;
        Unit  = unit;
    }

    /// <summary>
    /// The size mode, indicating how the <see cref="Value"/> should be interpreted.
    /// </summary>
    public EColumnUnit Unit { get; init; }

    /// <summary>
    /// The value of the size.
    /// </summary>
    public float Value { get; init; }

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

        EColumnUnit lengthMode;
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
            case "%":
                lengthMode = EColumnUnit.Percent;
                break;
            case "px":
            case "Px":
            case "pX":
            case "PX":
                lengthMode = EColumnUnit.Pixel;
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
                lengthMode = EColumnUnit.Auto;
                break;
            default:
                result = default;
                return false;
        }

        if (number.IsEmpty && lengthMode is not EColumnUnit.Auto and EColumnUnit.Part)
        {
            result = default;
            return false;
        }

        var value = number.IsEmpty ? 1F : float.Parse(number, provider);
        result = new ColumnLength(value, lengthMode);
        return true;
    }

    /// <inheritdoc />
    public static ColumnLength Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        s = s.Trim();
        if (s.IsEmpty)
            throw new ArgumentException("The string must not be empty.", nameof(s));

        EColumnUnit lengthMode;
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
                    throw new FormatException("The string contains more than one dot.");
                dotFound = true;
                continue;
            }

            endOfNumber = i;
            break;
        }

        var number = s[..endOfNumber];
        var unit = s[endOfNumber..].Trim();
        if (unit.IsEmpty)
            throw new FormatException("The string does not contain a unit.");

        switch (unit)
        {
            case "%":
                lengthMode = EColumnUnit.Percent;
                break;
            case "*":
                lengthMode = EColumnUnit.Part;
                break;
            case "px":
            case "Px":
            case "pX":
            case "PX":
                lengthMode = EColumnUnit.Pixel;
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
                lengthMode = EColumnUnit.Auto;
                break;
            default:
                throw new NotSupportedException($"The unit '{unit}' is not supported.");
        }

        if (number.IsEmpty && lengthMode is not EColumnUnit.Auto and EColumnUnit.Part)
            throw new FormatException("The string does not contain a number.");
        var value = number.IsEmpty ? 1F : float.Parse(number, provider);
        if (lengthMode is EColumnUnit.Percent)
            value /= 100.0F;
        return new ColumnLength(value, lengthMode);
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
        var unit = Unit switch
        {
            EColumnUnit.Auto => "auto",
            EColumnUnit.Part => "*",
            EColumnUnit.Percent => "%",
            EColumnUnit.Pixel => "px",
            _ => throw new InvalidEnumArgumentException(nameof(Unit), (int) Unit, typeof(EColumnUnit)),
        };
        return Unit is EColumnUnit.Auto
            ? unit
            : $"{Value.ToString(provider)}{unit}";
    }
}