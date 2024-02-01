using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Attributes;
using X39.Solutions.PdfTemplate.Controls.Base;
using X39.Solutions.PdfTemplate.Data;
using X39.Solutions.PdfTemplate.Services.TextService;

namespace X39.Solutions.PdfTemplate.Controls;

/// <summary>
/// Base class for all controls with the intention to display simple text.
/// </summary>
public abstract class TextBaseControl : AlignableControl
{
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
    /// The rotation of the text, default is 0°.
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
        return TextService.Measure(TextStyle, dpi, GetText().AsSpan().Trim(), remainingSize.Width);
    }

    /// <inheritdoc />
    protected override Size DoArrange(
        float       dpi,
        in Size     fullPageSize,
        in Size     framedPageSize,
        in Size     remainingSize,
        CultureInfo cultureInfo)
    {
        return TextService.Measure(TextStyle, dpi, GetText().AsSpan().Trim(), remainingSize.Width);
    }

    /// <inheritdoc />
    protected override Size DoRender(ICanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        RenderText(canvas, dpi, GetText().Trim());
        return Size.Zero;
    }

    /// <summary>
    /// Renders the specified text on a drawable canvas with the specified text style settings.
    /// </summary>
    /// <param name="canvas">The drawable canvas on which to render the text.</param>
    /// <param name="dpi">The dots per inch value.</param>
    /// <param name="text">The text to render.</param>
    protected void RenderText(IDrawableCanvas canvas, float dpi, string text)
    {
        TextService.Draw(canvas, TextStyle, dpi, text.AsSpan(), ArrangementInner.Width);
    }
}
