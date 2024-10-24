using X39.Solutions.PdfTemplate.Abstraction;

namespace X39.Solutions.PdfTemplate.Attributes;

/// <summary>
/// Attribute to explicitly mark a constructor for use when creating a <see cref="IParameterConverter{T}"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Constructor)]
public class ParameterConverterConstructorAttribute : Attribute;