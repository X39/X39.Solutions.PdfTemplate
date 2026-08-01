namespace X39.Solutions.Papercraft;

/// <summary>
/// Describes a renderer backend and the output/features it supports.
/// </summary>
public sealed class RendererCapabilities
{
    /// <summary>
    /// Creates renderer capabilities.
    /// </summary>
    public RendererCapabilities(
        string rendererId,
        string displayName,
        RendererOutputKind outputKinds,
        IEnumerable<string> mediaTypes,
        IReadOnlyDictionary<string, RendererSupportLevel>? featureSupport = null,
        string? notes = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rendererId);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentNullException.ThrowIfNull(mediaTypes);
        RendererId = rendererId;
        DisplayName = displayName;
        OutputKinds = outputKinds;
        MediaTypes = mediaTypes.Select((q) => q?.Trim())
                               .Where((q) => !string.IsNullOrWhiteSpace(q))
                               .Select((q) => q!)
                               .Distinct(StringComparer.OrdinalIgnoreCase)
                               .ToArray();
        FeatureSupport = featureSupport is null
            ? new Dictionary<string, RendererSupportLevel>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, RendererSupportLevel>(featureSupport, StringComparer.OrdinalIgnoreCase);
        Notes = notes;
    }

    /// <summary>
    /// Stable renderer identifier.
    /// </summary>
    public string RendererId { get; }

    /// <summary>
    /// Human-readable renderer name.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Supported output kinds.
    /// </summary>
    public RendererOutputKind OutputKinds { get; }

    /// <summary>
    /// Supported media types.
    /// </summary>
    public IReadOnlyCollection<string> MediaTypes { get; }

    /// <summary>
    /// Renderer feature support map.
    /// </summary>
    public IReadOnlyDictionary<string, RendererSupportLevel> FeatureSupport { get; }

    /// <summary>
    /// Backend-specific notes.
    /// </summary>
    public string? Notes { get; }

    /// <summary>
    /// Checks whether the renderer supports the target output kind and media type.
    /// </summary>
    /// <param name="target">The target to check.</param>
    /// <returns><see langword="true"/> if the renderer supports the target.</returns>
    public bool Supports(RenderTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return SupportsOutputKind(target.OutputKind)
               && SupportsMediaTypeValue(target.MediaType);
    }

    /// <summary>
    /// Checks whether the renderer supports an output kind.
    /// </summary>
    /// <param name="outputKind">The output kind to check.</param>
    /// <returns><see langword="true"/> if the renderer supports the output kind.</returns>
    public bool SupportsOutputKind(RendererOutputKind outputKind)
        => outputKind is not RendererOutputKind.None
           && (OutputKinds & outputKind) == outputKind;

    /// <summary>
    /// Checks whether the renderer supports a media type.
    /// </summary>
    /// <param name="mediaType">The media type to check.</param>
    /// <returns><see langword="true"/> if the renderer supports the media type.</returns>
    public bool SupportsMediaType(string mediaType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mediaType);
        return SupportsMediaTypeValue(mediaType);
    }

    /// <summary>
    /// Validates the requested target against the renderer capabilities.
    /// </summary>
    /// <param name="target">The target to validate.</param>
    /// <returns>The target validation result.</returns>
    public RenderValidationResult ValidateTarget(RenderTarget target)
    {
        var diagnostics = GetTargetDiagnostics(target);
        return diagnostics.Count is 0
            ? RenderValidationResult.Supported
            : new RenderValidationResult(diagnostics);
    }

    /// <summary>
    /// Validates declared renderer feature constraints against a prepared document.
    /// </summary>
    /// <param name="document">The generated document to validate.</param>
    /// <returns>The document feature validation result.</returns>
    public RenderValidationResult ValidateDocument(PapercraftDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var diagnostics = GetFeatureDiagnostics(document.FeatureUses);
        return diagnostics.Count is 0
            ? RenderValidationResult.Supported
            : new RenderValidationResult(diagnostics);
    }

    /// <summary>
    /// Gets diagnostics explaining why a target is not supported by the renderer.
    /// </summary>
    /// <param name="target">The target to validate.</param>
    /// <returns>Diagnostics for unsupported target aspects, or an empty list if the target is supported.</returns>
    public IReadOnlyList<RenderDiagnostic> GetTargetDiagnostics(RenderTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);
        var diagnostics = new List<RenderDiagnostic>();
        if (!SupportsOutputKind(target.OutputKind))
        {
            diagnostics.Add(
                new RenderDiagnostic(
                    RenderDiagnosticCodes.UnsupportedOutputKind,
                    RendererSupportLevel.Unsupported,
                    RendererFeatures.ForOutputKind(target.OutputKind),
                    $"Renderer '{DisplayName}' does not support output kind '{target.OutputKind}' for media type '{target.MediaType}'.",
                    $"Supported output kinds are: {FormatOutputKinds(OutputKinds)}."));
        }

        if (!SupportsMediaTypeValue(target.MediaType))
        {
            diagnostics.Add(
                new RenderDiagnostic(
                    RenderDiagnosticCodes.UnsupportedMediaType,
                    RendererSupportLevel.Unsupported,
                    RendererFeatures.ForOutputKind(target.OutputKind),
                    $"Renderer '{DisplayName}' does not support media type '{target.MediaType}' for output kind '{target.OutputKind}'.",
                    $"Supported media types are: {FormatMediaTypes()}."));
        }

        return diagnostics;
    }

    /// <summary>
    /// Gets support for a feature.
    /// </summary>
    /// <param name="feature">The feature name.</param>
    /// <returns>The support level. Unknown features are unsupported.</returns>
    public RendererSupportLevel GetFeatureSupport(string feature)
        => FeatureSupport.TryGetValue(feature, out var supportLevel)
            ? supportLevel
            : RendererSupportLevel.Unsupported;

    /// <summary>
    /// Gets diagnostics for template features that this renderer declares as degraded or unsupported.
    /// </summary>
    /// <param name="featureUses">The template feature uses to validate.</param>
    /// <returns>Diagnostics for declared degraded or unsupported feature support.</returns>
    public IReadOnlyList<RenderDiagnostic> GetFeatureDiagnostics(IEnumerable<RenderFeatureUse> featureUses)
    {
        ArgumentNullException.ThrowIfNull(featureUses);
        var diagnostics = new List<RenderDiagnostic>();
        foreach (var featureUse in featureUses)
        {
            if (!FeatureSupport.TryGetValue(featureUse.Feature, out var supportLevel))
            {
                // Attachments must be opted into explicitly: silently omitting them would lose document content.
                // Preserve the established validation behavior for other undeclared features.
                if (!string.Equals(featureUse.Feature, RendererFeatures.EmbeddedFiles, StringComparison.Ordinal))
                    continue;

                supportLevel = RendererSupportLevel.Unsupported;
            }

            if (supportLevel is RendererSupportLevel.Supported)
                continue;

            var code = supportLevel is RendererSupportLevel.Unsupported
                ? RenderDiagnosticCodes.UnsupportedFeature
                : RenderDiagnosticCodes.DegradedFeature;
            var message = supportLevel is RendererSupportLevel.Unsupported
                ? $"Renderer '{DisplayName}' does not support template feature '{featureUse.Feature}'."
                : $"Renderer '{DisplayName}' supports template feature '{featureUse.Feature}' with degraded output.";
            diagnostics.Add(
                new RenderDiagnostic(
                    code,
                    supportLevel,
                    featureUse.Feature,
                    message,
                    $"Renderer capability '{featureUse.Feature}' is {supportLevel}.",
                    featureUse.Location));
        }

        return diagnostics;
    }

    private bool SupportsMediaTypeValue(string? mediaType)
    {
        if (string.IsNullOrWhiteSpace(mediaType))
            return false;
        var normalized = mediaType.Trim();
        return MediaTypes.Any((q) => string.Equals(q, normalized, StringComparison.OrdinalIgnoreCase));
    }

    private static string FormatOutputKinds(RendererOutputKind outputKinds)
    {
        if (outputKinds is RendererOutputKind.None)
            return RendererOutputKind.None.ToString();

        var values = Enum.GetValues<RendererOutputKind>()
                         .Where((q) => q is not RendererOutputKind.None
                                       && outputKinds.HasFlag(q))
                         .Select((q) => q.ToString())
                         .ToArray();
        return values.Length is 0
            ? outputKinds.ToString()
            : string.Join(", ", values);
    }

    private string FormatMediaTypes()
        => MediaTypes.Count is 0
            ? "none"
            : string.Join(", ", MediaTypes);
}
