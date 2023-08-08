namespace X39.Solutions.PdfTemplate.Attributes;

/// <summary>
/// Describes a control.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ControlAttribute : Attribute
{
    /// <summary>
    /// Creates a new instance of <see cref="ControlAttribute"/>.
    /// </summary>
    /// <remarks>
    /// Namespace will be derived from the namespace of the class.
    /// Name will be derived from the class name.
    /// </remarks>
    public ControlAttribute()
    {
        Namespace = string.Empty;
        Name      = string.Empty;
    }

    /// <summary>
    /// Creates a new instance of <see cref="ControlAttribute"/>.
    /// </summary>
    /// <remarks>
    /// Name will be derived from the class name.
    /// </remarks>
    /// <param name="namespace">The namespace of the control</param>
    public ControlAttribute(string @namespace) : this(@namespace, string.Empty)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="ControlAttribute"/>.
    /// </summary>
    /// <param name="namespace">The namespace of the control</param>
    /// <param name="name">The name of the control</param>
    public ControlAttribute(string @namespace, string name)
    {
        if (@namespace.TakeWhile(char.IsWhiteSpace).Any())
            throw new ArgumentException("Namespace must not start with whitespace.", nameof(@namespace));
        if (@namespace.Reverse().TakeWhile(char.IsWhiteSpace).Any())
            throw new ArgumentException("Namespace must not end with whitespace.", nameof(@namespace));
        if (name.TakeWhile(char.IsWhiteSpace).Any())
            throw new ArgumentException("Name must not start with whitespace.", nameof(@namespace));
        if (name.Reverse().TakeWhile(char.IsWhiteSpace).Any())
            throw new ArgumentException("Name must not end with whitespace.", nameof(@namespace));
        Namespace = @namespace;
        Name = Validators.ControlName.IsValid(name)
            ? name
            : throw new ArgumentException($"Invalid control name {name}.", nameof(name));
    }

    /// <summary>
    /// The name of the control.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The namespace of the control.
    /// </summary>
    public string Namespace { get; }
}