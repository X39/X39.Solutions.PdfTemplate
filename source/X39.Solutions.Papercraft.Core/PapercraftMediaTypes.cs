namespace X39.Solutions.Papercraft;

/// <summary>
/// Common media type constants understood by Papercraft renderers.
/// </summary>
public static class PapercraftMediaTypes
{
    /// <summary>
    /// Arbitrary binary content.
    /// </summary>
    public const string ApplicationOctetStream = "application/octet-stream";

    /// <summary>
    /// PDF document output.
    /// </summary>
    public const string ApplicationPdf = "application/pdf";

    /// <summary>
    /// PNG raster image output.
    /// </summary>
    public const string ImagePng = "image/png";

    /// <summary>
    /// Plain text content.
    /// </summary>
    public const string TextPlain = "text/plain";

    /// <summary>
    /// Lowered Papercraft XML output.
    /// </summary>
    public const string ApplicationPapercraftLoweredXml = "application/vnd.papercraft.lowered+xml";
}
