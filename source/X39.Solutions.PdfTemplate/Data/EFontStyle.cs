namespace X39.Solutions.PdfTemplate.Data;

/// <summary>
/// The style of the font.
/// </summary>
public enum EFontStyle
{
    /// <summary>
    /// The upright/normal font slant.
    /// </summary>
    Upright,
    
    /// <summary>
    /// Alias for <see cref="Upright"/>.
    /// </summary>
    Normal = Upright,

    /// <summary>
    /// The italic font slant, in which the slanted characters appear as they were designed.
    /// </summary>
    Italic,

    /// <summary>
    /// The oblique font slant, in which the characters are artificially slanted.
    /// </summary>
    Oblique,
}