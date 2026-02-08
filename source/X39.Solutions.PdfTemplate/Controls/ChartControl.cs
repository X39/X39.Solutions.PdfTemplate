using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Abstraction.Controls;
using X39.Solutions.PdfTemplate.Attributes;
using X39.Solutions.PdfTemplate.Controls.Base;
using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate.Controls;

/// <summary>
/// Wrapper control for charts.
/// </summary>
[Control(Constants.ControlsNamespace, "chart")]
public class ChartControl: AlignableContentControl
{
    private readonly List<Size> _arrangedSizes = new();

    /// <inheritdoc />
    protected override Size DoMeasure(float dpi, in Size fullPageSize, in Size framedPageSize, in Size remainingSize, CultureInfo cultureInfo)
    {
        var maxWidth = 0f;
        var totalHeight = 0f;

        foreach (var child in Children)
        {
            var childSize = child.Measure(dpi, fullPageSize, framedPageSize, remainingSize, cultureInfo);
            maxWidth = Math.Max(maxWidth, childSize.Width);
            totalHeight += childSize.Height;
        }

        return new Size(maxWidth, totalHeight);
    }

    /// <inheritdoc />
    protected override Size DoArrange(float dpi, in Size fullPageSize, in Size framedPageSize, in Size remainingSize, CultureInfo cultureInfo)
    {
        _arrangedSizes.Clear();
        var maxWidth = 0f;
        var totalHeight = 0f;

        foreach (var child in Children)
        {
            var childSize = child.Arrange(dpi, fullPageSize, framedPageSize, remainingSize, cultureInfo);
            _arrangedSizes.Add(childSize);
            maxWidth = Math.Max(maxWidth, childSize.Width);
            totalHeight += childSize.Height;
        }

        return new Size(maxWidth, totalHeight);
    }

    /// <inheritdoc />
    protected override Size DoRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        foreach (var (child, arrangedSize) in Children.Zip(_arrangedSizes))
        {
            child.Render(canvas, dpi, parentSize, cultureInfo);
            canvas.Translate(0, arrangedSize.Height);
        }

        return Size.Zero;
    }

    /// <inheritdoc />
    public override bool CanAdd(Type type)
        => type.IsAssignableTo(typeof(IChart));
}