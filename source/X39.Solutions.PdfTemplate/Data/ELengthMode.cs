namespace X39.Solutions.PdfTemplate.Data;

/// <summary>
/// Enum for the size mode of a <see cref="Length"/>
/// </summary>
public enum ELengthMode
{
    /// <summary>
    /// The size is in pixels.
    /// </summary>
    Pixel,
    
    /// <summary>
    /// The size is in percent of the available space.
    /// </summary>
    Percent,
    
    /// <summary>
    /// The size is in points.
    /// </summary>
    /// <remarks>
    /// 1 point = 1/72.272 inch
    /// </remarks>
    Points,
}