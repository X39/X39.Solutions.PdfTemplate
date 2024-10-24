using X39.Solutions.PdfTemplate.Attributes;

namespace X39.Solutions.PdfTemplate.Abstraction;

/// <summary>
/// Interface for a converter that converts a <see cref="string"/> to a
/// specific type.
/// </summary>
/// <remarks>
/// This interface is used by the <see cref="Generator"/> to convert
/// parameters from the template to the type of the property of the
/// <see cref="IControl"/>. To make a parameter converter available to the
/// <see cref="Generator"/>, set the type of the <see cref="ParameterAttribute.Converter"/>.
/// </remarks>
/// <typeparam name="T">The type to convert.</typeparam>
public interface IParameterConverter<T>
{
    /// <summary>
    /// Converts the <paramref name="value"/> to the desired type.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="format">A format specifier if available.</param>
    /// <param name="cultureInfo">The culture to use for the conversion.</param>
    /// <returns>The converted value.</returns>
    T Convert(string value, string? format, CultureInfo cultureInfo);
}