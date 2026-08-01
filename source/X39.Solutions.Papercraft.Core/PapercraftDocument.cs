using X39.Solutions.Papercraft.Display;

namespace X39.Solutions.Papercraft;

/// <summary>
/// A backend-neutral generated Papercraft document.
/// </summary>
public sealed class PapercraftDocument
{
    /// <summary>
    /// Creates a generated Papercraft document.
    /// </summary>
    public PapercraftDocument(
        IReadOnlyList<PapercraftPage> pages,
        CultureInfo cultureInfo,
        DocumentOptions documentOptions)
    {
        ArgumentNullException.ThrowIfNull(pages);
        ArgumentNullException.ThrowIfNull(cultureInfo);
        Pages = pages;
        CultureInfo = cultureInfo;
        DocumentOptions = documentOptions with
        {
            EmbeddedFiles = documentOptions.EmbeddedFiles.ToArray(),
        };
        FeatureUses = AnalyzeFeatureUses(pages, DocumentOptions);
    }

    /// <summary>
    /// Generated pages.
    /// </summary>
    public IReadOnlyList<PapercraftPage> Pages { get; }

    /// <summary>
    /// Culture used during generation.
    /// </summary>
    public CultureInfo CultureInfo { get; }

    /// <summary>
    /// Document options used during generation.
    /// </summary>
    public DocumentOptions DocumentOptions { get; }

    /// <summary>
    /// Renderer-relevant features used by this document.
    /// </summary>
    public IReadOnlyList<RenderFeatureUse> FeatureUses { get; }

    private static IReadOnlyList<RenderFeatureUse> AnalyzeFeatureUses(
        IReadOnlyList<PapercraftPage> pages,
        DocumentOptions documentOptions)
    {
        var features = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var page in pages)
        {
            foreach (var command in page.DisplayList.Commands)
            {
                AddCommandFeatures(features, command);
            }
        }

        if (pages.Count > 1)
            features.Add(RendererFeatures.Multipage);

        if (documentOptions.EmbeddedFiles.Count is not 0)
            features.Add(RendererFeatures.EmbeddedFiles);

        return features
            .OrderBy((q) => q, StringComparer.OrdinalIgnoreCase)
            .Select((q) => new RenderFeatureUse(q))
            .ToArray();
    }

    private static void AddCommandFeatures(HashSet<string> features, DisplayCommand command)
    {
        switch (command)
        {
            case DrawImageCommand:
                features.Add(RendererFeatures.Images);
                features.Add(RendererFeatures.AbsolutePositioning);
                break;
            case DrawTextCommand drawText:
                features.Add(RendererFeatures.TextDrawing);
                features.Add(RendererFeatures.TextMeasurement);
                features.Add(RendererFeatures.Fonts);
                features.Add(RendererFeatures.Color);
                features.Add(RendererFeatures.AbsolutePositioning);
                if (drawText.TextStyle.Foreground.Alpha < byte.MaxValue)
                    features.Add(RendererFeatures.Transparency);
                break;
            case DrawLineCommand drawLine:
                features.Add(RendererFeatures.Color);
                features.Add(RendererFeatures.AbsolutePositioning);
                if (drawLine.Color.Alpha < byte.MaxValue)
                    features.Add(RendererFeatures.Transparency);
                break;
            case DrawRectangleCommand drawRectangle:
                features.Add(RendererFeatures.Color);
                features.Add(RendererFeatures.AbsolutePositioning);
                if (drawRectangle.Color.Alpha < byte.MaxValue)
                    features.Add(RendererFeatures.Transparency);
                break;
            case ClipCommand:
                features.Add(RendererFeatures.Clipping);
                break;
            case LinkAnnotationCommand:
                features.Add(RendererFeatures.LinkAnnotations);
                features.Add(RendererFeatures.AbsolutePositioning);
                break;
        }
    }
}
