using X39.Solutions.PdfTemplate.Abstraction;

namespace X39.Solutions.PdfTemplate.Attributes;

/// <summary>
/// Allows to specify a default converter for a class when used as a parameter for <see cref="IControl"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
[MeansImplicitUse(ImplicitUseKindFlags.Assign)]
public class ParameterConverterAttribute<TConverter> : ParameterConverterAttributeBase
{
    /// <inheritdoc />
    public override Type Converter => typeof(TConverter);
}