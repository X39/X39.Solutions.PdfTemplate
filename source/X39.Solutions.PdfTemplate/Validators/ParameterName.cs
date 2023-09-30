using System.Reflection;
using X39.Solutions.PdfTemplate.Attributes;
using X39.Util.Collections;

namespace X39.Solutions.PdfTemplate.Validators;

/// <summary>
/// Validator for control parameter names.
/// </summary>
public static class ParameterName
{
    /// <summary>
    /// Checks if the given name is valid.
    /// </summary>
    /// <param name="name">The name to check.</param>
    /// <returns><see langword="true"/> if the name is valid, <see langword="false"/> otherwise.</returns>
    public static bool IsValid(string name) => name.None((c) => c is '.' or '<' or '>' or ':');

    /// <summary>
    /// Returns the name of the given type.
    /// </summary>
    /// <param name="parameterAttribute"></param>
    /// <param name="propertyInfo"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static string Get(ParameterAttribute parameterAttribute, PropertyInfo propertyInfo)
    {
        var name = parameterAttribute.Name;
        if (name is null)
        {
            const string parameterSuffix = "Parameter";
            name = propertyInfo.Name;
            if (name.EndsWith(parameterSuffix, StringComparison.Ordinal))
                name = name[..^parameterSuffix.Length];
        }

        if (!IsValid(name))
            throw new InvalidOperationException(
                $"The name {name} of property {propertyInfo.Name} in {propertyInfo.DeclaringType?.FullName()} is not valid. It must not contain any of the following characters: . < > :");
        return name.ToUpperInvariant();
    }
}