using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Services.TextService;

namespace X39.Solutions.Papercraft.Controls;

/// <summary>
/// A visual hyperlink control that renders styled text and an optional underline.
/// </summary>
[Control(Constants.ControlsNamespace, "hyperlink")]
public sealed class HyperlinkControl : TextBaseControl
{
    private const float PageBoundaryTolerance = 0.001F;

    private bool _underline = true;

    /// <summary>
    /// Creates a new instance of <see cref="HyperlinkControl"/>.
    /// </summary>
    /// <param name="textService">The text service to use.</param>
    [ControlConstructor]
    public HyperlinkControl(ITextService textService) : base(textService)
    {
        Foreground = Colors.Blue;
        Decoration = TextDecoration.Underline;
    }

    /// <summary>
    /// The hyperlink target.
    /// </summary>
    [Parameter]
    public string Href { get; set; } = string.Empty;

    /// <summary>
    /// The text to display.
    /// </summary>
    [Parameter(IsContent = true)]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Whether the visible text should be underlined.
    /// </summary>
    [Parameter]
    public bool Underline
    {
        get => _underline;
        set
        {
            _underline = value;
            Decoration = value
                ? Decoration | TextDecoration.Underline
                : Decoration & ~(TextDecoration.Underline | TextDecoration.DoubleUnderline);
        }
    }

    /// <inheritdoc />
    protected override string GetText() => Text;

    /// <inheritdoc />
    protected override Size DoRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        var text = GetText().Trim();
        var pageHeight = canvas.PageSize.Height > 0F ? canvas.PageSize.Height : parentSize.Height;
        var additionalSize = RenderText(canvas, dpi, text, pageHeight);
        RenderLinkAnnotations(canvas, dpi, text, pageHeight);
        return additionalSize;
    }

    private void RenderLinkAnnotations(IDeferredCanvas canvas, float dpi, string text, float pageHeight)
    {
        var uri = Href.Trim();
        if (string.IsNullOrWhiteSpace(uri))
            return;

        var layout = GetTextLayout(dpi, text, ArrangementInner.Width);
        if (layout is null)
            return;

        var additionalHeight = 0F;
        foreach (var line in layout)
        {
            var lineAdditionalHeight = CalculateLinePaginationAdditionalHeight(
                canvas.Translation.Y + line.Top + additionalHeight,
                line.Height,
                pageHeight);
            additionalHeight += lineAdditionalHeight;

            if (string.IsNullOrWhiteSpace(line.Text))
                continue;

            canvas.DrawLinkAnnotation(
                uri,
                new Rectangle(
                    line.X,
                    line.Top + additionalHeight,
                    line.Width,
                    line.Height));
        }
    }

    private static float CalculateLinePaginationAdditionalHeight(
        float absoluteLineTop,
        float lineHeight,
        float pageHeight)
    {
        if (pageHeight <= 0F || lineHeight <= 0F || lineHeight > pageHeight + PageBoundaryTolerance)
            return 0F;

        var usedHeight = GetUsedPageHeight(absoluteLineTop, pageHeight);
        if (usedHeight <= PageBoundaryTolerance)
            return 0F;

        if (usedHeight + lineHeight <= pageHeight + PageBoundaryTolerance)
            return 0F;

        var remainingPageHeight = pageHeight - usedHeight;
        return remainingPageHeight <= PageBoundaryTolerance
            ? 0F
            : remainingPageHeight;
    }

    private static float GetUsedPageHeight(float y, float pageHeight)
    {
        var multiplier = (int)(y / pageHeight);
        return y - multiplier * pageHeight;
    }
}
