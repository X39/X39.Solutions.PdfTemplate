using System.Reflection;
using X39.Solutions.PdfTemplate.Attributes;
using X39.Util.Collections;

namespace X39.Solutions.PdfTemplate.Validators;

/// <summary>
/// Validator for control names.
/// </summary>
public static class ControlName
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
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static string Get(Type type)
    {
        if (type.IsGenericType)
            throw new InvalidOperationException(
                $"The type {type.FullName} is a generic type and cannot be used as a control.");
        var attribute = type.GetCustomAttribute<ControlAttribute>();
        if (attribute is null)
            throw new InvalidOperationException(
                $"The type {type.FullName()} does not have a {nameof(ControlAttribute)}.");
        var name = attribute.Name;
        if (!IsValid(name))
            throw new InvalidOperationException(
                $"The name {name} of {type.FullName()} is not valid. It must not contain any of the following characters: . < > :");
        if (!name.IsNullOrEmpty())
            return name;
        const string controlSuffix = "Control";
        name = type.Name();

        if (name.EndsWith(controlSuffix, StringComparison.Ordinal))
            name = name[..^controlSuffix.Length];

        return name;
    }
}