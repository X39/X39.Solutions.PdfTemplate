﻿using System.ComponentModel;
using System.Globalization;

namespace X39.Solutions.PdfTemplate.Data;

/// <inheritdoc />
public sealed class ThicknessConverter : TypeConverter
{
    /// <inheritdoc />
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType.IsEquivalentTo(typeof(string)) || base.CanConvertFrom(context, sourceType);

    /// <inheritdoc />
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        => (destinationType?.IsEquivalentTo(typeof(string)) ?? false) || base.CanConvertTo(context, destinationType);

    /// <inheritdoc />
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is not string source)
            return base.ConvertFrom(context, culture, value);
        return Thickness.Parse(source, culture);
    }

    /// <inheritdoc />
    public override object? ConvertTo(
        ITypeDescriptorContext? context,
        CultureInfo? culture,
        object? value,
        Type destinationType)
    {
        if (value is not Thickness thickness || !destinationType.IsEquivalentTo(typeof(string)))
            return base.ConvertTo(context, culture, value, destinationType);
        return thickness.ToString(culture);
    }
}