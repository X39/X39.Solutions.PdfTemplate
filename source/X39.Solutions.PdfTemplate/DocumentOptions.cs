using X39.Solutions.PdfTemplate.Data;

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

    /// <summary>
    /// The date and time the document was most recently modified.
    /// </summary>
    public DateTime Modified { get; set; } = DateTime.Now;

    /// <summary>
    /// The product that is converting this document to PDF.
    /// </summary>
    public string Producer { get; set; } = "";
    
    /// <summary>
    /// The margin of the document.
    /// </summary>
    /// <remarks>
    /// Margin is removed from the page size, not added to it.
    /// This implies that a margin of 100pt... or 10pt... will result in the same page size,
    /// but the content will be moved by 100pt... or 10pt... respectively.
    /// </remarks>
    public Thickness Margin { get; init; } = new(0, 0, 0, 0);
}