using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Attributes;
using X39.Solutions.PdfTemplate.Controls.Base;
using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate.Controls;

/// <summary>
/// The table cell control.
/// </summary>
[Control(Constants.ControlsNamespace, "td")]
public sealed class TableCellControl : AlignableContentControl
{
    /// <summary>
    /// The width of the column.
    /// </summary>
    [Parameter]
    public ColumnLength Width { get; set; } = new();

    /// <summary>
    /// Gets or sets the number of columns that the table cell spans.
    /// </summary>
    /// <remarks>
    /// The <see cref="ColumnSpan"/> property indicates the number of columns that the table cell spans.
    /// A table cell with a <see cref="ColumnSpan"/> of 1 occupies a single column.
    /// A table cell with a larger <see cref="ColumnSpan"/> value spans multiple columns and takes up the width
    /// of those columns.
    /// A table cell with a <see cref="ColumnSpan"/> of 0 spans will be ignored.
    /// </remarks>
    [Parameter]
    public ushort ColumnSpan { get; set; } = 1;

    private readonly List<float> _heights = new();

    /// <inheritdoc />
    protected override Size DoMeasure(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        var width = 0F;
        var height = 0F;
        foreach (var control in Children)
        {
            var size = control.Measure(dpi, fullPageSize, remainingSize, remainingSize, cultureInfo);
            width  =  Math.Max(width, size.Width);
            height += size.Height;
        }

        return new Size(width, height);
    }

    /// <inheritdoc />
    protected override Size DoArrange(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        var width = 0F;
        var height = 0F;
        foreach (var control in Children)
        {
            var size = control.Arrange(dpi, fullPageSize, remainingSize, remainingSize, cultureInfo);
            width  =  Math.Max(width, size.Width);
            height += size.Height;
            _heights.Add(size.Height);
        }

        if (HorizontalAlignment == EHorizontalAlignment.Stretch)
            width = Math.Max(width, remainingSize.Width);
        if (VerticalAlignment == EVerticalAlignment.Stretch)
            height = Math.Max(height, remainingSize.Height);
        return new Size(Math.Min(width, framedPageSize.Width), height);
    }

    /// <inheritdoc />
    protected override Size DoRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        var additionalWidth  = 0F;
        var additionalHeight = 0F;
        using (canvas.CreateState())
        {
            foreach (var (child, childHeight) in Children.Zip(_heights))
            {
                var (width, height) =  child.Render(canvas, dpi, parentSize, cultureInfo);
                additionalWidth     += width;
                additionalHeight    += height;
                canvas.Translate(0, childHeight);
            }
        }
        return new Size(additionalWidth, additionalHeight);
    }

    /// <inheritdoc />
    public override bool CanAdd(Type type) => true;
}