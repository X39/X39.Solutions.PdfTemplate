namespace X39.Solutions.PdfTemplate.Data;

/// <summary>
/// Common font weights.
/// </summary>
[PublicAPI]
public static class FontWidths
{
    internal static readonly TypeExpressionMapCache<FontWidth> AccessCache = new(typeof(FontWidths));
    /// <summary>
    /// A condensed font width of 1.
    /// </summary>
    public static FontWidth UltraCondensed { get; } = new(1);

    /// <summary>
    /// A condensed font width of 2.
    /// </summary>
    public static FontWidth ExtraCondensed { get; } = new(2);

    /// <summary>
    /// A condensed font width of 3.
    /// </summary>
    public static FontWidth Condensed { get; } = new(3);

    /// <summary>
    /// A condensed font width of 4.
    /// </summary>
    public static FontWidth SemiCondensed { get; } = new(4);

    /// <summary>
    /// A normal font width of 5. This is the default font width.
    /// </summary>
    public static FontWidth Normal { get; } = new(5);

    /// <summary>
    /// An expanded font width of 6.
    /// </summary>
    public static FontWidth SemiExpanded { get; } = new(6);

    /// <summary>
    /// An expanded font width of 7.
    /// </summary>
    public static FontWidth Expanded { get; } = new(7);

    /// <summary>
    /// An expanded font width of 8.
    /// </summary>
    public static FontWidth ExtraExpanded { get; } = new(8);

    /// <summary>
    /// An expanded font width of 9.
    /// </summary>
    public static FontWidth UltraExpanded { get; } = new(9);
}