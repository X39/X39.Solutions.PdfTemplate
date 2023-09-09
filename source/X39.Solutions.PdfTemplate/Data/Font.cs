using SkiaSharp;

namespace X39.Solutions.PdfTemplate.Data;

/// <summary>
/// Represents a font
/// </summary>
/// <param name="Family">The font family</param>
public readonly record struct Font(string Family)
{
    static Font()
    {
        if (OperatingSystem.IsWindows())
            Default = new Font("Arial");
        else if (OperatingSystem.IsLinux())
        {
            using var typeFace = SKTypeface.CreateDefault();
            Default = new Font(typeFace.FamilyName);
        }
    }

    /// <summary>
    /// Default font style.
    /// </summary>
    public static Font Default { get; }

    /// <summary>
    /// The width or letter-spacing of the font
    /// </summary>
    public FontWidth LetterSpacing { get; init; }

    /// <summary>
    /// The weight of the font
    /// </summary>
    public FontWeight Weight { get; init; }

    /// <summary>
    /// The style of the font.
    /// </summary>
    public EFontStyle Style { get; init; }
}