namespace X39.Solutions.Papercraft;

/// <summary>
/// Well-known renderer feature names used by validation diagnostics.
/// </summary>
public static class RendererFeatures
{
    /// <summary>
    /// PDF output.
    /// </summary>
    public const string PdfOutput = "output.pdf";

    /// <summary>
    /// Raster image output.
    /// </summary>
    public const string RasterImageOutput = "output.raster-image";

    /// <summary>
    /// Vector image output.
    /// </summary>
    public const string VectorImageOutput = "output.vector-image";

    /// <summary>
    /// Printer command output.
    /// </summary>
    public const string PrinterCommandOutput = "output.printer-commands";

    /// <summary>
    /// Backend-defined custom output.
    /// </summary>
    public const string CustomOutput = "output.custom";

    /// <summary>
    /// Lowered XML output.
    /// </summary>
    public const string LoweredXmlOutput = "output.lowered-xml";

    /// <summary>
    /// Multiple rendered pages in one document.
    /// </summary>
    public const string Multipage = "document.multipage";

    /// <summary>
    /// Text measuring support.
    /// </summary>
    public const string TextMeasurement = "text.measurement";

    /// <summary>
    /// Text drawing support.
    /// </summary>
    public const string TextDrawing = "text.drawing";

    /// <summary>
    /// Image drawing support.
    /// </summary>
    public const string Images = "image.drawing";

    /// <summary>
    /// Rectangular clipping support.
    /// </summary>
    public const string Clipping = "drawing.clipping";

    /// <summary>
    /// Alpha/transparency support.
    /// </summary>
    public const string Transparency = "drawing.transparency";

    /// <summary>
    /// Font family, style, width, and weight support.
    /// </summary>
    public const string Fonts = "text.fonts";

    /// <summary>
    /// Color output support.
    /// </summary>
    public const string Color = "drawing.color";

    /// <summary>
    /// Absolute positioning support.
    /// </summary>
    public const string AbsolutePositioning = "layout.absolute-positioning";

    /// <summary>
    /// Clickable link annotation support.
    /// </summary>
    public const string LinkAnnotations = "pdf.link-annotations";

    /// <summary>
    /// Embedded file (document attachment) support.
    /// </summary>
    public const string EmbeddedFiles = "pdf.embedded-files";

    /// <summary>
    /// Gets the feature name for a render output kind.
    /// </summary>
    /// <param name="outputKind">The render output kind.</param>
    /// <returns>The feature name for the output kind.</returns>
    public static string ForOutputKind(RendererOutputKind outputKind)
        => outputKind switch
        {
            RendererOutputKind.Pdf => PdfOutput,
            RendererOutputKind.RasterImage => RasterImageOutput,
            RendererOutputKind.VectorImage => VectorImageOutput,
            RendererOutputKind.PrinterCommands => PrinterCommandOutput,
            RendererOutputKind.Custom => CustomOutput,
            RendererOutputKind.LoweredXml => LoweredXmlOutput,
            _ => CustomOutput,
        };
}
