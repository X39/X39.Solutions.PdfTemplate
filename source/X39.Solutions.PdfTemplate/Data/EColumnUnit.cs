namespace X39.Solutions.PdfTemplate.Data;

/// <summary>
/// Enum for the size mode of a <see cref="ColumnLength"/>.
/// </summary>
public enum EColumnUnit
{
    /// <summary>
    /// The size is automatically calculated.
    /// </summary>
    Auto,

    /// <summary>
    /// The size is in pixels.
    /// </summary>
    Pixel,

    /// <summary>
    /// The size is in parts of the available space.
    /// </summary>
    Part,

    /// <summary>
    /// The size is in percent of the available space.
    /// </summary>
    Percent,
}