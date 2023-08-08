using X39.Solutions.PdfTemplate.Attributes;

namespace X39.Solutions.PdfTemplate.Data.Compound;

/// <summary>
/// A collection of properties attributed with <see cref="ParameterAttribute"/> for use in a <see cref="IControl"/>,
/// representing font styling information.
/// </summary>
public readonly record struct FontStyleCompound
{
    /// <summary>
    /// The name of the font.
    /// </summary>
    /// <remarks>
    /// If this is null, the default font will be used.
    /// </remarks>
    [Parameter(Name = "Family")]
    public string? Family { get; init; }

    /// <summary>
    /// The foreground color of the text.
    /// </summary>
    [Parameter(Name = "Color")]
    public Color? Color { get; init; }

    /// <summary>
    /// The size of the text.
    /// </summary>
    public FontWeight? Weight { get; init; }

    /// <summary>
    /// Whether the text is italic.
    /// </summary>
    public bool? IsItalic { get; init; }
    
    /// <summary>
    /// Whether the text is underlined.
    /// </summary>
    public bool? IsUnderlined { get; init; }
    
    /// <summary>
    /// Whether the text is crossed out.
    /// </summary>
    public bool? IsCrossedOut { get; init; }
}