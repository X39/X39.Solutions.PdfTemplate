namespace X39.Solutions.PdfTemplate;

/// <summary>
/// Configuration options for the document.
/// </summary>
[PublicAPI]
public record struct DocumentOptions()
{
    /// <summary>
    /// The default configuration options for the document.
    /// </summary>
    public static DocumentOptions Default => new();
    
    /// <summary>
    /// The DPI of the document.
    /// </summary>
    public float DotsPerInch { get; init; } = 96;
    
    /// <summary>
    /// The DPCM of the document.
    /// </summary>
    public float DotsPerCentimeter
    {
        get => DotsPerInch / 2.54f;
        init => DotsPerInch = value * 2.54f;
    }
    
    /// <summary>
    /// The DPMM of the document.
    /// </summary>
    public float DotsPerMillimeter
    {
        get => DotsPerInch / 25.4f;
        init => DotsPerInch = value * 25.4f;
    }

    /// <summary>
    /// The width and height of the document in millimeters.
    /// </summary>
    public float PageWidthInMillimeters { get; init; } = 210;
    
    /// <summary>
    /// The width and height of the document in millimeters.
    /// </summary>
    public float PageHeightInMillimeters { get; init; } = 297;
}