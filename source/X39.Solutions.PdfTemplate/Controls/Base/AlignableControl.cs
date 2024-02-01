using System.ComponentModel;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Attributes;
using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate.Controls.Base;

/// <summary>
/// Base class for all controls that can be aligned horizontally and vertically
/// </summary>
public abstract class AlignableControl : Control
{
    /// <summary>
    /// The horizontal alignment of the control.
    /// </summary>
    [Parameter]
    public EHorizontalAlignment HorizontalAlignment { get; set; } = EHorizontalAlignment.Stretch;

    /// <summary>
    /// The vertical alignment of the control.
    /// </summary>
    [Parameter]
    public EVerticalAlignment VerticalAlignment { get; set; } = EVerticalAlignment.Stretch;

    /// <inheritdoc />
    protected override Size PreRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        canvas.Translate(
            new Point
            {
                X = HorizontalAlignment switch
                {
                    EHorizontalAlignment.Left    => 0,
                    EHorizontalAlignment.Center  => RemainingSize.Width / 2 -  Arrangement.Width / 2,
                    EHorizontalAlignment.Right   => RemainingSize.Width - Arrangement.Width,
                    EHorizontalAlignment.Stretch => 0,
                    _ => throw new InvalidEnumArgumentException(
                        nameof(HorizontalAlignment),
                        (int) HorizontalAlignment,
                        typeof(EHorizontalAlignment)),
                },
                Y = VerticalAlignment switch
                {
                    EVerticalAlignment.Top     => 0,
                    EVerticalAlignment.Center  => RemainingSize.Height / 2 - Arrangement.Height / 2,
                    EVerticalAlignment.Bottom  => RemainingSize.Height - Arrangement.Height,
                    EVerticalAlignment.Stretch => 0,
                    _ => throw new InvalidEnumArgumentException(
                        nameof(VerticalAlignment),
                        (int) VerticalAlignment,
                        typeof(EVerticalAlignment)),
                },
            });
        return Size.Zero;
    }
}