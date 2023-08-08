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
        in Size availableSize,
        CultureInfo cultureInfo)
    {
        return Orientation switch
        {
            EOrientation.Horizontal => new Size(
                Length.ToPixels(availableSize.Width),
                Thickness.ToPixels(availableSize.Height)),
            EOrientation.Vertical => new Size(
                Thickness.ToPixels(availableSize.Width),
                Length.ToPixels(availableSize.Height)),
            _ => throw new InvalidEnumArgumentException(nameof(Orientation), (int) Orientation, typeof(EOrientation)),
        };
    }

    /// <inheritdoc />
    protected override Size DoArrange(
        in Size finalSize,
        CultureInfo cultureInfo)
    {
        return Orientation switch
        {
            EOrientation.Horizontal => new Size(
                Length.ToPixels(finalSize.Width),
                Thickness.ToPixels(finalSize.Height)),
            EOrientation.Vertical => new Size(
                Thickness.ToPixels(finalSize.Width),
                Length.ToPixels(finalSize.Height)),
            _ => throw new InvalidEnumArgumentException(nameof(Orientation), (int) Orientation, typeof(EOrientation)),
        };
    }


    /// <inheritdoc />
    protected override void DoRender(
        ICanvas canvas,
        in Size parentSize,
        CultureInfo cultureInfo)
    {
        var length = Length.ToPixels(
            Orientation is EOrientation.Horizontal
                ? parentSize.Width
                : parentSize.Height);
        var thickness = Thickness.ToPixels(
            Orientation is EOrientation.Horizontal
                ? parentSize.Height
                : parentSize.Width);
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