using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Controls.Base;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Services.TextService;

namespace X39.Solutions.Papercraft.Controls;

/// <summary>
/// Base class for all controls with the intention to display simple text.
/// </summary>
public abstract class TextBaseControl : AlignableControl
{
    private const float PageBoundaryTolerance = 0.001F;
    private TextLayoutCacheEntry? _textLayoutCacheEntry;

    /// <summary>
    /// The text service passed in the constructor.
    /// </summary>
    protected ITextService TextService { get; }

    /// <summary>
    /// Creates a new instance of <see cref="TextControl"/>
    /// </summary>
    /// <param name="textService">The text service to use.</param>#
    public TextBaseControl(ITextService textService)
    {
        TextService = textService;
    }

    /// <summary>
    /// Gets the text to be displayed by the control.
    /// </summary>
    /// <returns>The text to be displayed.</returns>
    protected abstract string GetText();

    /// <summary>
    /// The text style represented by this control.
    /// </summary>
    protected TextStyle TextStyle { get; private set; } = new();


    /// <summary>
    /// The foreground color of the text
    /// </summary>
    [Parameter]
    public Color Foreground
    {
        get => TextStyle.Foreground;
        set => TextStyle = TextStyle with
        {
            Foreground = value,
        };
    }

    /// <summary>
    /// The size of the text.
    /// </summary>
    [Parameter]
    public float FontSize
    {
        get => TextStyle.FontSize;
        set => TextStyle = TextStyle with
        {
            FontSize = value,
        };
    }


    /// <summary>
    /// The size of the text.
    /// </summary>
    [Parameter]
    public float LineHeight
    {
        get => TextStyle.LineHeight;
        set => TextStyle = TextStyle with
        {
            LineHeight = value,
        };
    }

    /// <summary>
    /// The scale of the text, default is 1.
    /// </summary>
    [Parameter]
    public float Scale
    {
        get => TextStyle.Scale;
        set => TextStyle = TextStyle with
        {
            Scale = value,
        };
    }

    /// <summary>
    /// The rotation of the text, default is 0 degrees.
    /// </summary>
    [Parameter]
    public float Rotation
    {
        get => TextStyle.Rotation;
        set => TextStyle = TextStyle with
        {
            Rotation = value,
        };
    }

    /// <summary>
    /// The thickness of the stroke for the <see cref="Foreground"/> color.
    /// </summary>
    [Parameter]
    public float StrokeThickness
    {
        get => TextStyle.StrokeThickness;
        set => TextStyle = TextStyle with
        {
            StrokeThickness = value,
        };
    }

    /// <summary>
    /// Decorations applied to the text.
    /// </summary>
    [Parameter]
    public TextDecoration Decoration
    {
        get => TextStyle.Decoration;
        set => TextStyle = TextStyle with
        {
            Decoration = value,
        };
    }

    /// <summary>
    /// The width or letter-spacing of the font
    /// </summary>
    [Parameter]
    public FontWidth LetterSpacing
    {
        get => TextStyle.FontFamily.LetterSpacing;
        set => TextStyle = TextStyle with
        {
            FontFamily = TextStyle.FontFamily with
            {
                LetterSpacing = value,
            },
        };
    }

    /// <summary>
    /// The weight of the font
    /// </summary>
    [Parameter]
    public FontWeight Weight
    {
        get => TextStyle.FontFamily.Weight;
        set => TextStyle = TextStyle with
        {
            FontFamily = TextStyle.FontFamily with
            {
                Weight = value,
            },
        };
    }

    /// <summary>
    /// The style of the font.
    /// </summary>
    [Parameter]
    public EFontStyle Style
    {
        get => TextStyle.FontFamily.Style;
        set => TextStyle = TextStyle with
        {
            FontFamily = TextStyle.FontFamily with
            {
                Style = value,
            },
        };
    }

    /// <summary>
    /// The font family.
    /// </summary>
    [Parameter]
    public string FontFamily
    {
        get => TextStyle.FontFamily.Family;
        set => TextStyle = TextStyle with
        {
            FontFamily = TextStyle.FontFamily with
            {
                Family = value,
            },
        };
    }
    
    
    internal TextStyle GetTextStyle() => TextStyle;

    /// <inheritdoc />
    protected override Size DoMeasure(
        float       dpi,
        in Size     fullPageSize,
        in Size     framedPageSize,
        in Size     remainingSize,
        CultureInfo cultureInfo)
    {
        return MeasureText(dpi, GetText().Trim(), remainingSize.Width);
    }

    /// <inheritdoc />
    protected override Size DoArrange(
        float       dpi,
        in Size     fullPageSize,
        in Size     framedPageSize,
        in Size     remainingSize,
        CultureInfo cultureInfo)
    {
        var text = GetText().Trim();
        var size = MeasureText(dpi, text, remainingSize.Width);
        _ = GetTextLayout(dpi, text, size.Width);
        return size;
    }

    /// <inheritdoc />
    protected override Size PreRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        var baseAdditionalSize = base.PreRender(canvas, dpi, parentSize, cultureInfo);
        var layout = GetTextLayout(dpi, GetText().Trim(), ArrangementInner.Width);
        if (layout is null)
            return baseAdditionalSize;

        var additionalHeight = CalculatePaginationAdditionalHeight(
            canvas.Translation.Y + ArrangementInner.Top,
            GetPaginationPageHeight(canvas, parentSize),
            layout);
        return new Size(
            baseAdditionalSize.Width,
            baseAdditionalSize.Height + additionalHeight);
    }

    /// <inheritdoc />
    protected override Size DoRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        return RenderText(canvas, dpi, GetText().Trim(), GetPaginationPageHeight(canvas, parentSize));
    }

    private static float GetPaginationPageHeight(IDeferredCanvas canvas, in Size parentSize)
        => canvas.PageSize.Height > 0F ? canvas.PageSize.Height : parentSize.Height;

    /// <summary>
    /// Renders the specified text on a drawable canvas with the specified text style settings.
    /// </summary>
    /// <param name="canvas">The drawable canvas on which to render the text.</param>
    /// <param name="dpi">The dots per inch value.</param>
    /// <param name="text">The text to render.</param>
    /// <param name="pageHeight">The height of the current pageable area.</param>
    /// <returns>Additional vertical space inserted to keep rendered lines whole.</returns>
    protected Size RenderText(IDrawableCanvas canvas, float dpi, string text, float pageHeight = 0F)
    {
        if (canvas is IDeferredCanvas deferredCanvas
            && GetTextLayout(dpi, text, ArrangementInner.Width) is { } layout)
        {
            return RenderPaginatedText(deferredCanvas, dpi, layout, pageHeight);
        }

        TextService.Draw(canvas, TextStyle, dpi, text.AsSpan(), ArrangementInner.Width);
        return Size.Zero;
    }

    private Size MeasureText(float dpi, string text, float maxWidth)
        => TextService.Measure(TextStyle, dpi, text.AsSpan(), maxWidth);

    private protected IReadOnlyList<TextLineLayout>? GetTextLayout(float dpi, string text, float maxWidth)
    {
        if (TextService is not ITextLayoutService textLayoutService)
            return null;

        var key = new TextLayoutCacheKey(text, TextStyle, dpi, maxWidth);
        if (_textLayoutCacheEntry is { } entry && entry.Key == key)
            return entry.Layout;

        var layout = textLayoutService.Layout(TextStyle, dpi, text.AsSpan(), maxWidth);
        _textLayoutCacheEntry = new TextLayoutCacheEntry(key, layout);
        return layout;
    }

    private Size RenderPaginatedText(
        IDeferredCanvas canvas,
        float dpi,
        IReadOnlyList<TextLineLayout> layout,
        float pageHeight)
    {
        var additionalHeight = 0F;
        foreach (var line in layout)
        {
            var lineAdditionalHeight = CalculateLinePaginationAdditionalHeight(
                canvas.Translation.Y + line.Top + additionalHeight,
                line.Height,
                pageHeight);
            additionalHeight += lineAdditionalHeight;
            canvas.DrawText(TextStyle, dpi, line.Text, line.X, line.BaselineY + additionalHeight);
        }

        return new Size(0F, additionalHeight);
    }

    private static float CalculatePaginationAdditionalHeight(
        float absoluteTextTop,
        float pageHeight,
        IReadOnlyList<TextLineLayout> layout)
    {
        var additionalHeight = 0F;
        foreach (var line in layout)
        {
            additionalHeight += CalculateLinePaginationAdditionalHeight(
                absoluteTextTop + line.Top + additionalHeight,
                line.Height,
                pageHeight);
        }

        return additionalHeight;
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

    private readonly record struct TextLayoutCacheKey(
        string Text,
        TextStyle TextStyle,
        float Dpi,
        float MaxWidth);

    private sealed record TextLayoutCacheEntry(
        TextLayoutCacheKey Key,
        IReadOnlyList<TextLineLayout> Layout);
}
