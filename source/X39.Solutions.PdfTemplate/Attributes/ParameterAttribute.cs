using System.ComponentModel;

namespace X39.Solutions.PdfTemplate.Attributes;

/// <summary>
/// If a property of a <see cref="IControl"/> is marked with this attribute,
/// the <see cref="Generator"/> will pass the value of the property in the
/// template to the control.
/// If a property accepts sub controls, the property must be marked with this attribute and the
/// property type must be assignable to a <see cref="ICollection{T}"/> where <c>T</c> is a <see cref="IControl"/>.
/// </summary>
/// <remarks>
/// The property will be converted from a <see cref="string"/> to the type
/// using the converter received via <see cref="TypeDescriptor"/> or the
/// one specified in <see cref="Converter"/>.
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
[MeansImplicitUse(ImplicitUseKindFlags.Assign)]
public class ParameterAttribute : Attribute
{
    /// <summary>
    /// Allows to specify a custom name for the parameter.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The type of the converter to use to convert the parameter.
    /// </summary>
    public Type? Converter { get; set; }
    
    /// <summary>
    /// A format specifier for the converter.
    /// </summary>
    /// <remarks>
    /// Note that the converter must support the format specifier and that templates may
    /// specify a format specifier on their own, taking precedence over this value.
    /// </remarks>
    public string? Format { get; set; }
    
    /// <summary>
    /// If set to <c>true</c>, the parameter may be filled from the content in the template node.
    /// </summary>
    /// <remarks>
    /// If a parameter is both, provided in the template node and as an attribute, a validation error will be raised
    /// to a user, indicating that the parameter is ambiguous.
    /// </remarks>
    /// <example>
    /// <myControl>
    /// This will be used to fill the parameter if <see cref="IsContent"/> is set to <c>true</c>.
    /// </myControl>
    /// </example>
    public bool IsContent { get; set; }
}