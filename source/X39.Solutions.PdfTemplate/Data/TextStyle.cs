namespace X39.Solutions.PdfTemplate.Data;

/// <summary>
/// Represents styling information for text
/// </summary>
public readonly record struct TextStyle()
{
    /// <summary>
    /// The foreground color of the text
    /// </summary>
    public Color Foreground { get; init; } = Colors.Black;

    /// <summary>
    /// The size of the text.
    /// </summary>
    public float FontSize { get; init; } = 12F;

    /// <summary>
    /// The font family to use.
    /// </summary>
    public Font FontFamily { get; init; } = Font.Default;

    /// <summary>
    /// The scale of the text, default is 1.
    /// </summary>
    public float Scale { get; init; } = 1F;

    /// <summary>
    /// The height of a line, default is 1.
    /// </summary>
    /// <remarks>
    /// This value is dependent on the font size.
    /// </remarks>
    public float LineHeight { get; init; } = 1F;

    /// <summary>
    /// The rotation of the text, default is 0°.
    /// </summary>
    public float Rotation { get; init; } = 0F;
    
    /// <summary>
    /// The thickness of the stroke for the <see cref="Foreground"/> color.
    /// </summary>
    public float StrokeThickness { get; init; } = 1F;
}