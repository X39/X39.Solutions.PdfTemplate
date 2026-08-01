using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.Papercraft;

/// <summary>
/// Configuration options for the document.
/// </summary>
public record struct DocumentOptions()
{
    /// <summary>
    /// The default configuration options for the document.
    /// </summary>
    public static DocumentOptions Default => new();

    /// <summary>
    /// Additional consumer-defined context for the current document generation request.
    /// </summary>
    /// <remarks>
    /// The context is opaque to the library and passed unchanged to context-aware extension points.
    /// </remarks>
    public object? Context { get; set; }

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

    /// <summary>
    /// If set to true, instructs the generator to ignore any error that may occur.
    /// </summary>
    /// <remarks>
    /// Depending on the exact position, this may lead to invalid xml, preventing the printing anyways.
    /// </remarks>
    public bool IgnoreErrors { get; set; }

    /// <summary>
    /// The files attached to the document.
    /// </summary>
    /// <remarks>
    /// Renderers declaring <see cref="RendererFeatures.EmbeddedFiles"/> support write these into their
    /// output; renderers that cannot carry attachments report a diagnostic instead of dropping them
    /// silently. Never <see langword="null"/>, including for <see langword="default"/> instances.
    /// </remarks>
    public IReadOnlyList<EmbeddedFile> EmbeddedFiles
    {
        get => _embeddedFiles ?? Array.Empty<EmbeddedFile>();
        init => _embeddedFiles = value ?? throw new ArgumentNullException(nameof(value));
    }

    private readonly IReadOnlyList<EmbeddedFile>? _embeddedFiles;
}
