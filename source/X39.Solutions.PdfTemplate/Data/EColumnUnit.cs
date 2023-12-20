namespace X39.Solutions.PdfTemplate.Data;

/// <summary>
/// Enum for the size mode of a <see cref="ColumnLength"/>.
/// </summary>
public enum EColumnUnit
{
    /// <summary>
    /// The size is in parts of the available space.
    /// </summary>
    Parts,

    /// <summary>
    /// The size is specified in <see cref="Length"/>.
    /// </summary>
    Lenght,
}