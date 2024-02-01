using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Attributes;
using X39.Solutions.PdfTemplate.Controls.Base;
using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate.Controls;

/// <summary>
/// The border control.
/// </summary>
[Control(Constants.ControlsNamespace, "border")]
public class BorderControl : AlignableContentControl
{
    /// <summary>
    /// The thickness of the border.
    /// </summary>
    [Parameter]
    public Thickness Thickness { get; set; }

    /// <summary>
    /// The color of the border.
    /// </summary>
    [Parameter]
    public Color Color { get; set; }

    /// <summary>
    /// The background color of the border.
    /// </summary>
    [Parameter]
    public Color Background { get; set; }
    
    private List<Size> _arrangedSizes = new();

    /// <inheritdoc />
    protected override Size DoMeasure(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        var thickness       = Thickness.ToRectangle(fullPageSize, dpi);
        var thicknessOffset = new Size(thickness.Left, thickness.Top) + new Size(thickness.Width, thickness.Height);
        var size            = Size.Zero;
        foreach (var child in Children)
        {
            var measure = child.Measure(
                dpi,
                fullPageSize,
                remainingSize - thicknessOffset,
                remainingSize - thicknessOffset,
                cultureInfo
            );
            size = new Size(
                Math.Max(size.Width, measure.Width),
                size.Height + measure.Height);
        }
        var result = size + thicknessOffset;
        // if (HorizontalAlignment is EHorizontalAlignment.Stretch)
        //     result = result with {Width = Math.Max(result.Width, framedPageSize.Width)};
        // if (VerticalAlignment is EVerticalAlignment.Stretch)
        //     result = result with {Height = Math.Max(result.Height, framedPageSize.Height)};
        return result;
    }

    /// <inheritdoc />
    protected override Size DoArrange(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        var thickness     = Thickness.ToRectangle(fullPageSize, dpi);
        var thicknessOffset = new Size(thickness.Left, thickness.Top) + new Size(thickness.Width, thickness.Height);
        var size          = Size.Zero;
        foreach (var child in Children)
        {
            var measure = child.Arrange(
                dpi,
                fullPageSize,
                remainingSize - thicknessOffset,
                remainingSize - thicknessOffset,
                cultureInfo
            );
            size = new Size(
                Math.Max(size.Width, measure.Width),
                size.Height + measure.Height);
            _arrangedSizes.Add(measure);
        }
        var result = size + thicknessOffset;
        if (HorizontalAlignment is EHorizontalAlignment.Stretch)
            result = result with {Width = Math.Max(result.Width, remainingSize.Width)};
        if (VerticalAlignment is EVerticalAlignment.Stretch)
            result = result with {Height = Math.Max(result.Height, remainingSize.Height)};
        return result;
    }

    /// <inheritdoc />
    protected override Size DoRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        using var state = canvas.CreateState();
        var additionalWidth = 0F;
        var additionalHeight = 0F;
        canvas.Translate(-ArrangementInner);
        canvas.Translate(Arrangement);
        var thickness = Thickness.ToRectangle(parentSize, dpi);
        if (Background != Colors.Transparent)
            canvas.DrawRect(Arrangement with {Left = 0, Top = 0}, Background);
        if (thickness.Left > 0)
            canvas.DrawLine(
                Color,
                thickness.Left * 2,
                0,
                0,
                0,
                Arrangement.Height);
        if (thickness.Top > 0)
            canvas.DrawLine(
                Color,
                thickness.Top * 2,
                0,
                0,
                Arrangement.Width,
                0);
        if (thickness.Right > 0)
            canvas.DrawLine(
                Color,
                thickness.Width * 2,
                Arrangement.Width,
                Arrangement.Height,
                Arrangement.Width,
                0);
        if (thickness.Bottom > 0)
            canvas.DrawLine(
                Color,
                thickness.Height * 2,
                Arrangement.Width,
                Arrangement.Height,
                0,
                Arrangement.Height);

        canvas.Translate(-Arrangement);
        canvas.Translate(ArrangementInner);
        canvas.Translate(thickness);
        foreach (var (child, arrangedSize) in Children.Zip(_arrangedSizes))
        {
            var (width, height) = child.Render(canvas, dpi, parentSize, cultureInfo);
            additionalWidth += width;
            additionalHeight += height;
            canvas.Translate(0, arrangedSize.Height);
        }

        return new Size(additionalWidth, additionalHeight);
    }

    /// <inheritdoc />
    public override bool CanAdd(Type type) => true;
}