namespace X39.Solutions.PdfTemplate.Attributes;

/// <summary>
/// Attribute to explicitly mark a constructor for use when creating a <see cref="IControl"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Constructor)]
public class ControlConstructorAttribute : Attribute
{
}