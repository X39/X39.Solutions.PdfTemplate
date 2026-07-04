using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Canvas;
using X39.Solutions.Papercraft.Controls.Base;
using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.Papercraft.Controls;

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
    
    private readonly List<Size> _arrangedSizes = new();
    private Size _preRenderAdditionalSize;

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
        _arrangedSizes.Clear();
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
    protected override Size PreRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        var baseAdditionalSize = base.PreRender(canvas, dpi, parentSize, cultureInfo);
        _preRenderAdditionalSize = Size.Zero;
        if (!Clip)
            return baseAdditionalSize;

        var dryRunCanvas = DryRunDeferredCanvas.From(canvas);
        var thickness = Thickness.ToRectangle(parentSize, dpi);
        dryRunCanvas.Translate(ArrangementInner);
        dryRunCanvas.Translate(thickness);
        var contentAdditionalSize = RenderChildren(
            dryRunCanvas,
            dpi,
            parentSize,
            cultureInfo);
        _preRenderAdditionalSize = contentAdditionalSize;

        return new Size(
            Math.Max(baseAdditionalSize.Width, contentAdditionalSize.Width),
            baseAdditionalSize.Height + contentAdditionalSize.Height);
    }

    /// <inheritdoc />
    protected override Size DoRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        using var state = canvas.CreateState();
        canvas.Translate(-ArrangementInner);
        canvas.Translate(Arrangement);
        var thickness = Thickness.ToRectangle(parentSize, dpi);
        var renderedArrangement = Arrangement + _preRenderAdditionalSize;
        if (Background != Colors.Transparent)
            canvas.DrawRect(renderedArrangement with {Left = 0, Top = 0}, Background);
        if (thickness.Left > 0)
            DrawBorderRectangle(
                canvas,
                new Rectangle(
                    0,
                    0,
                    Math.Min(thickness.Left, renderedArrangement.Width),
                    renderedArrangement.Height));
        if (thickness.Top > 0)
            DrawBorderRectangle(
                canvas,
                new Rectangle(
                    0,
                    0,
                    renderedArrangement.Width,
                    Math.Min(thickness.Top, renderedArrangement.Height)));
        if (thickness.Width > 0)
            DrawBorderRectangle(
                canvas,
                new Rectangle(
                    Math.Max(0, renderedArrangement.Width - thickness.Width),
                    0,
                    Math.Min(thickness.Width, renderedArrangement.Width),
                    renderedArrangement.Height));
        if (thickness.Height > 0)
            DrawBorderRectangle(
                canvas,
                new Rectangle(
                    0,
                    Math.Max(0, renderedArrangement.Height - thickness.Height),
                    renderedArrangement.Width,
                    Math.Min(thickness.Height, renderedArrangement.Height)));

        canvas.Translate(-Arrangement);
        canvas.Translate(ArrangementInner);
        canvas.Translate(thickness);
        return RenderChildren(canvas, dpi, parentSize, cultureInfo);
    }

    private Size RenderChildren(
        IDeferredCanvas canvas,
        float dpi,
        in Size parentSize,
        CultureInfo cultureInfo)
    {
        var additionalWidth = 0F;
        var additionalHeight = 0F;
        foreach (var (child, arrangedSize) in Children.Zip(_arrangedSizes))
        {
            var (width, height) = child.Render(canvas, dpi, parentSize, cultureInfo);
            additionalWidth += width;
            additionalHeight += height;
            canvas.Translate(0, arrangedSize.Height + height);
        }

        return new Size(additionalWidth, additionalHeight);
    }

    private void DrawBorderRectangle(IDeferredCanvas canvas, Rectangle rectangle)
    {
        if (rectangle is not { Width: > 0, Height: > 0 })
            return;
        canvas.DrawRect(rectangle, Color);
    }

    /// <inheritdoc />
    public override bool CanAdd(Type type) => true;
}
