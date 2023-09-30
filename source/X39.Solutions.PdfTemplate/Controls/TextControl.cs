using System.Globalization;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Attributes;
using X39.Solutions.PdfTemplate.Controls.Base;
using X39.Solutions.PdfTemplate.Data;
using X39.Solutions.PdfTemplate.Services.TextService;

namespace X39.Solutions.PdfTemplate.Controls;

/// <summary>
/// A control that displays text.
/// </summary>
[Control(Constants.ControlsNamespace)]
public class TextControl : AlignableControl
{
    private readonly ITextService _textService;

    /// <summary>
    /// Creates a new instance of <see cref="TextControl"/>
    /// </summary>
    /// <param name="textService">The text service to use.</param>#
    [ControlConstructor]
    public TextControl(ITextService textService)
    {
        _textService = textService;
    }

    /// <summary>
    /// The text to display.
    /// </summary>
    [Parameter(IsContent = true)]
    public string Text { get; set; } = string.Empty;

    private TextStyle _textStyle = new();


    /// <summary>
    /// The foreground color of the text
    /// </summary>
    [Parameter]
    public Color Foreground
    {
        get => _textStyle.Foreground;
        set => _textStyle = _textStyle with
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
        get => _textStyle.FontSize;
        set => _textStyle = _textStyle with
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
        get => _textStyle.LineHeight;
        set => _textStyle = _textStyle with
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
        get => _textStyle.Scale;
        set => _textStyle = _textStyle with
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
        get => _textStyle.Rotation;
        set => _textStyle = _textStyle with
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
        get => _textStyle.StrokeThickness;
        set => _textStyle = _textStyle with
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
        get => _textStyle.FontFamily.LetterSpacing;
        set => _textStyle = _textStyle with
        {
            FontFamily = _textStyle.FontFamily with
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
        get => _textStyle.FontFamily.Weight;
        set => _textStyle = _textStyle with
        {
            FontFamily = _textStyle.FontFamily with
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
        get => _textStyle.FontFamily.Style;
        set => _textStyle = _textStyle with
        {
            FontFamily = _textStyle.FontFamily with
            {
                Style = value,
            },
        };
    }

    /// <inheritdoc />
    protected override Size DoMeasure(
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        return _textService.Measure(_textStyle, Text.AsSpan(), remainingSize.Width);
    }

    /// <inheritdoc />
    protected override Size DoArrange(
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        return _textService.Measure(_textStyle, Text.AsSpan(), remainingSize.Width);
    }

    /// <inheritdoc />
    protected override void DoRender(ICanvas canvas, in Size parentSize, CultureInfo cultureInfo)
    {
        _textService.Draw(canvas, _textStyle, Text.AsSpan(), ArrangementInner.Width);
    }
}