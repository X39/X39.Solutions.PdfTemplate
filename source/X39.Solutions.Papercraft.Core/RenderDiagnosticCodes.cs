namespace X39.Solutions.Papercraft;

/// <summary>
/// Stable diagnostic codes emitted by Papercraft validation.
/// </summary>
public static class RenderDiagnosticCodes
{
    /// <summary>
    /// The selected renderer does not support the requested output kind.
    /// </summary>
    public const string UnsupportedOutputKind = "PAPERCRAFT001";

    /// <summary>
    /// The selected renderer does not support the requested media type.
    /// </summary>
    public const string UnsupportedMediaType = "PAPERCRAFT002";

    /// <summary>
    /// The selected renderer does not support a feature used by the template.
    /// </summary>
    public const string UnsupportedFeature = "PAPERCRAFT003";

    /// <summary>
    /// The selected renderer supports a template feature only with degraded output.
    /// </summary>
    public const string DegradedFeature = "PAPERCRAFT004";

    /// <summary>
    /// The selected renderer substituted a missing requested font family.
    /// </summary>
    public const string MissingFontSubstitution = "PAPERCRAFT005";

    /// <summary>
    /// The selected renderer substituted a requested font face or style.
    /// </summary>
    public const string FontFaceSubstitution = "PAPERCRAFT006";

    /// <summary>
    /// An embedded file requested by the document cannot be written as specified.
    /// </summary>
    public const string InvalidEmbeddedFile = "PAPERCRAFT007";
}
