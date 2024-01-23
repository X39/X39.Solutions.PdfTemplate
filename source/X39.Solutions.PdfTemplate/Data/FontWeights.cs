using System.ComponentModel;

namespace X39.Solutions.PdfTemplate.Data;

/// <summary>
/// Common font weights.
/// </summary>
[PublicAPI]
public static class FontWeights
{
    internal static readonly TypeExpressionMapCache<FontWeight> AccessCache = new(typeof(FontWeights));
    /// <summary>
    /// Font weight 100 with common weight name 'Thin (Hairline)'.
    /// </summary>
    /// <seealso href="https://learn.microsoft.com/de-de/typography/opentype/spec/os2#usweightclass">Specification</seealso>
    public static FontWeight Thin { get; } = new(100);

    /// <summary>
    /// Font weight 200 with common weight name 'Extra Light (Ultra Light)'.
    /// </summary>
    /// <seealso href="https://learn.microsoft.com/de-de/typography/opentype/spec/os2#usweightclass">Specification</seealso>
    public static FontWeight ExtraLight { get; } = new(200);

    /// <summary>
    /// Font weight 300 with common weight name 'Light'.
    /// </summary>
    /// <seealso href="https://learn.microsoft.com/de-de/typography/opentype/spec/os2#usweightclass">Specification</seealso>
    public static FontWeight Light { get; } = new(300);

    /// <summary>
    /// Font weight 400 with common weight name 'Normal (Regular)'.
    /// </summary>
    /// <seealso href="https://learn.microsoft.com/de-de/typography/opentype/spec/os2#usweightclass">Specification</seealso>
    public static FontWeight Normal { get; } = new(400);

    /// <summary>
    /// Font weight 500 with common weight name 'Medium'.
    /// </summary>
    /// <seealso href="https://learn.microsoft.com/de-de/typography/opentype/spec/os2#usweightclass">Specification</seealso>
    public static FontWeight Medium { get; } = new(500);

    /// <summary>
    /// Font weight 600 with common weight name 'Semi Bold (Demi Bold)'.
    /// </summary>
    /// <seealso href="https://learn.microsoft.com/de-de/typography/opentype/spec/os2#usweightclass">Specification</seealso>
    public static FontWeight SemiBold { get; } = new(600);

    /// <summary>
    /// Font weight 700 with common weight name 'Bold'.
    /// </summary>
    /// <seealso href="https://learn.microsoft.com/de-de/typography/opentype/spec/os2#usweightclass">Specification</seealso>
    public static FontWeight Bold { get; } = new(700);

    /// <summary>
    /// Font weight 800 with common weight name 'Extra Bold (Ultra Bold)'.
    /// </summary>
    /// <seealso href="https://learn.microsoft.com/de-de/typography/opentype/spec/os2#usweightclass">Specification</seealso>
    public static FontWeight ExtraBold { get; } = new(800);

    /// <summary>
    /// Font weight 900 with common weight name 'Black (Heavy)'.
    /// </summary>
    /// <seealso href="https://learn.microsoft.com/de-de/typography/opentype/spec/os2#usweightclass">Specification</seealso>
    public static FontWeight Black { get; } = new(900);
}