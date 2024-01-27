using System.Globalization;
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

        var w = Math.Min(width, remainingSize.Width);
        var h = Math.Min(height, remainingSize.Height);
        return new Size(
            HorizontalAlignment is EHorizontalAlignment.Stretch ? Math.Max(w, remainingSize.Width) : w,
            VerticalAlignment is EVerticalAlignment.Stretch ? Math.Max(h, remainingSize.Height) : h);
    }

    /// <inheritdoc />
    protected override void DoRender(ICanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        using (canvas.CreateState())
        {
            foreach (var (control, height) in Children.Zip(_heights))
            {
                control.Render(canvas, dpi, parentSize, cultureInfo);
                canvas.Translate(0, height);
            }
        }
    }

    /// <inheritdoc />
    public override bool CanAdd(Type type) => true;
}