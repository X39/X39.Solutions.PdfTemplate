using System.ComponentModel;

namespace X39.Solutions.PdfTemplate.Data;

/// <inheritdoc />
public sealed class FontWeightConverter : TypeConverter
{
    /// <inheritdoc />
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        TypeDescriptor.GetConverter(sourceType).CanConvertTo(typeof(ushort))
        || sourceType.IsEquivalentTo(typeof(string))
        || base.CanConvertFrom(context, sourceType);

    /// <inheritdoc />
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
        destinationType is not null
        && (TypeDescriptor.GetConverter(destinationType).CanConvertFrom(typeof(ushort))
            || TypeDescriptor.GetConverter(destinationType).CanConvertFrom(typeof(FontWeight))
            || destinationType.IsEquivalentTo(typeof(string))
            || base.CanConvertTo(context, destinationType)
        );

    /// <inheritdoc />
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object? value)
    {
        if (value is null) return base.ConvertFrom(context, culture, value!);
        if (value is ushort weight) return new FontWeight(weight);
        if (value is string source)
        {
            if (FontWeight.TryParse(source, culture, out var fontWeight)) return fontWeight;
            if (culture is null
                    ? FontWeights.AccessCache.TryGet(source, out fontWeight)
                    : FontWeights.AccessCache.TryGet(source, culture, out fontWeight))
                return fontWeight;
        }
        var valueType = value.GetType();
        if (TypeDescriptor.GetConverter(valueType).CanConvertTo(typeof(FontWeight)))
            return new FontWeight((FontWeight)TypeDescriptor.GetConverter(valueType).ConvertTo(value, typeof(FontWeight))!);
        if (TypeDescriptor.GetConverter(valueType).CanConvertTo(typeof(ushort)))
            return new FontWeight((ushort)(TypeDescriptor.GetConverter(valueType).ConvertTo(value, typeof(ushort)) ?? 0));
        return base.ConvertFrom(context, culture, value);
    }

    /// <inheritdoc />
    public override object? ConvertTo(
        ITypeDescriptorContext? context,
        CultureInfo?            culture,
        object?                 value,
        Type                    destinationType)
    {
        if (value is not FontWeight fontWeight)
            return base.ConvertTo(context, culture, value, destinationType);
        if (destinationType.IsEquivalentTo(typeof(string)))
            return fontWeight.ToString();
        var typeConverter = TypeDescriptor.GetConverter(destinationType);
        if (typeConverter.CanConvertFrom(typeof(FontWeight)))
            return typeConverter.ConvertFrom(fontWeight);
        if (typeConverter.CanConvertFrom(typeof(ushort)))
            return typeConverter.ConvertFrom(fontWeight.Value);
        return base.ConvertTo(context, culture, value, destinationType);
    }
}
