using System.Globalization;
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

    /// <inheritdoc />
    protected override Size DoMeasure(
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        var thickness = Thickness.ToRectangle(fullPageSize);
        var size = Children.FirstOrDefault()
                       ?.Measure(fullPageSize, framedPageSize, remainingSize - thickness, cultureInfo)
                   ?? Size.Zero;
        return size + new Size(thickness.Left, thickness.Top) + new Size(thickness.Width, thickness.Height);
    }

    /// <inheritdoc />
    protected override Size DoArrange(
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        var thickness = Thickness.ToRectangle(fullPageSize);
        var size = Children.FirstOrDefault()
                       ?.Arrange(fullPageSize, framedPageSize, remainingSize - thickness, cultureInfo)
                   ?? Size.Zero;
        return size + new Size(thickness.Left, thickness.Top) + new Size(thickness.Width, thickness.Height);
    }

    /// <inheritdoc />
    protected override void DoRender(ICanvas canvas, in Size parentSize, CultureInfo cultureInfo)
    {
        canvas.Translate(-ArrangementInner);
        canvas.Translate(Arrangement);
        var thickness = Thickness.ToRectangle(parentSize);
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
        using var state = canvas.CreateState();
        canvas.Translate(thickness);
        foreach (var child in Children)
        {
            child.Render(canvas, parentSize, cultureInfo);
        }
    }

    /// <inheritdoc />
    public override bool CanAdd(Type type) => Children.Count == 0;
}