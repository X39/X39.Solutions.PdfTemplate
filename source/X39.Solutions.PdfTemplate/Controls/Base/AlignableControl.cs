using System.ComponentModel;
using System.Globalization;
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
    protected override void PreRender(ICanvas canvas, in Size parentSize, CultureInfo cultureInfo)
    {
        var delta = parentSize - Arrangement;
        canvas.Translate(
            new Point
            {
                X = HorizontalAlignment switch
                {
                    EHorizontalAlignment.Left    => 0,
                    EHorizontalAlignment.Center  => delta.Width / 2,
                    EHorizontalAlignment.Right   => delta.Width,
                    EHorizontalAlignment.Stretch => 0,
                    _ => throw new InvalidEnumArgumentException(
                        nameof(HorizontalAlignment),
                        (int) HorizontalAlignment,
                        typeof(EHorizontalAlignment)),
                },
                Y = VerticalAlignment switch
                {
                    EVerticalAlignment.Top     => 0,
                    EVerticalAlignment.Center  => delta.Height / 2,
                    EVerticalAlignment.Bottom  => delta.Height,
                    EVerticalAlignment.Stretch => 0,
                    _ => throw new InvalidEnumArgumentException(
                        nameof(VerticalAlignment),
                        (int) VerticalAlignment,
                        typeof(EVerticalAlignment)),
                },
            });
    }
}