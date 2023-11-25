using System.ComponentModel;
using System.Globalization;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Attributes;
using X39.Solutions.PdfTemplate.Controls.Base;
using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate.Controls;

/// <summary>
/// Control that draws a line.
/// </summary>
[Control(Constants.ControlsNamespace)]
public sealed class LineControl : AlignableControl
{
    /// <summary>
    /// The thickness of the line drawn.
    /// </summary>
    [Parameter]
    public Length Thickness { get; set; } = new();

    /// <summary>
    /// The length of the line drawn.
    /// </summary>
    [Parameter]
    public Length Length { get; set; } = new();

    /// <summary>
    /// The orientation of the line drawn.
    /// </summary>
    [Parameter]
    public EOrientation Orientation { get; set; }

    /// <summary>
    /// The color of the line drawn.
    /// </summary>
    [Parameter]
    public Color Color { get; set; } = Colors.Black;

    /// <inheritdoc />
    protected override Size DoMeasure(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        return Orientation switch
        {
            EOrientation.Horizontal => new Size(
                Length.ToPixels(remainingSize.Width, dpi),
                Thickness.ToPixels(remainingSize.Height, dpi)),
            EOrientation.Vertical => new Size(
                Thickness.ToPixels(remainingSize.Width, dpi),
                Length.ToPixels(remainingSize.Height, dpi)),
            _ => throw new InvalidEnumArgumentException(nameof(Orientation), (int) Orientation, typeof(EOrientation)),
        };
    }

    /// <inheritdoc />
    protected override Size DoArrange(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        return Orientation switch
        {
            EOrientation.Horizontal => new Size(
                Length.ToPixels(remainingSize.Width, dpi),
                Thickness.ToPixels(remainingSize.Height, dpi)),
            EOrientation.Vertical => new Size(
                Thickness.ToPixels(remainingSize.Width, dpi),
                Length.ToPixels(remainingSize.Height, dpi)),
            _ => throw new InvalidEnumArgumentException(nameof(Orientation), (int) Orientation, typeof(EOrientation)),
        };
    }


    /// <inheritdoc />
    protected override void DoRender(
        ICanvas canvas,
        float dpi,
        in Size parentSize,
        CultureInfo cultureInfo)
    {
        var length = Length.ToPixels(
            Orientation is EOrientation.Horizontal
                ? parentSize.Width
                : parentSize.Height,
            dpi);
        var thickness = Thickness.ToPixels(
            Orientation is EOrientation.Horizontal
                ? parentSize.Height
                : parentSize.Width,
            dpi);
        switch (Orientation)
        {
            case EOrientation.Horizontal:
        canvas.DrawLine(
            Color,
            thickness,
            0,
            thickness / 2,
            length,
            thickness / 2);

                break;
            case EOrientation.Vertical:
        canvas.DrawLine(
            Color,
            thickness,
            thickness / 2,
            0,
            thickness / 2,
            length);
                break;
            default:
                throw new InvalidEnumArgumentException(nameof(Orientation), (int) Orientation, typeof(EOrientation));
        }
    }
}