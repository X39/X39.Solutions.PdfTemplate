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
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        var width = 0F;
        var height = 0F;
        foreach (var control in Children)
        {
            var size = control.Measure(fullPageSize, remainingSize, remainingSize, cultureInfo);
            width  =  Math.Max(width, size.Width);
            height += size.Height;
        }

        return new Size(width, height);
    }

    /// <inheritdoc />
    protected override Size DoArrange(
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        var width = 0F;
        var height = 0F;
        foreach (var control in Children)
        {
            var size = control.Arrange(fullPageSize, remainingSize, remainingSize, cultureInfo);
            width  =  Math.Max(width, size.Width);
            height += size.Height;
            _heights.Add(size.Height);
        }

        return new Size(
            Math.Min(width, remainingSize.Width), 
            Math.Min(height, remainingSize.Height));
    }

    /// <inheritdoc />
    protected override void DoRender(ICanvas canvas, in Size parentSize, CultureInfo cultureInfo)
    {
        canvas.PushState();
        foreach (var (control, height) in Children.Zip(_heights))
        {
            control.Render(canvas, parentSize, cultureInfo);
            canvas.Translate(0, height);
        }
        canvas.PopState();
    }

    /// <inheritdoc />
    public override bool CanAdd(Type type) => true;
}