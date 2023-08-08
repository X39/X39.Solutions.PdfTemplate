namespace X39.Solutions.PdfTemplate.Attributes;

/// <summary>
/// Allows to specify a default converter for a class when used as a parameter for <see cref="IControl"/>.
/// </summary>
#pragma warning disable CA1710
public abstract class ParameterConverterAttributeBase : Attribute
#pragma warning restore CA1710
{
    /// <summary>
    /// The converter type.
    /// </summary>
    public abstract Type Converter { get; }
}