using System.Globalization;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Controls.Base;
using X39.Solutions.PdfTemplate.Data;
using X39.Util.Collections;

namespace X39.Solutions.PdfTemplate.Controls;

/// <summary>
/// Base class for table row controls.
/// </summary>
public abstract class TableRowControlBase : AlignableContentControl
{
    internal TableControl? Table { get; set; }

    /// <inheritdoc />
    protected override Size DoMeasure(
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        if (Table is null)
            throw new InvalidOperationException("A TableRowControl must be added to a TableControl");
        var width = 0F;
        var height = 0F;
        foreach (var (control, index) in Children.Indexed())
        {
            var size = control.Measure(fullPageSize, framedPageSize, remainingSize, cultureInfo);
            width                   += size.Width;
            height                  =  Math.Max(height, size.Height);
            _                       =  Table.CellWidths.TryGetValue(index, out var cellWidth);
            Table.CellWidths[index] =  Math.Max(size.Width, cellWidth);
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
        if (Table is null)
            throw new InvalidOperationException("A TableRowControl must be added to a TableControl");
        var width = 0F;
        var height = 0F;
        foreach (var (control, index) in Children.Indexed())
        {
            _ = Table.CellWidths.TryGetValue(index, out var cellWidth);
            var remainingCellSize = remainingSize with {Width = cellWidth};
            var size = control.Arrange(fullPageSize, framedPageSize, remainingCellSize, cultureInfo);
            width  += cellWidth;
            height =  Math.Max(height, size.Height);
        }

        return new Size(width, height);
    }

    /// <inheritdoc />
    protected override void DoRender(ICanvas canvas, in Size parentSize, CultureInfo cultureInfo)
    {
        if (Table is null)
            throw new InvalidOperationException("A TableRowControl must be added to a TableControl");
        canvas.PushState();
        foreach (var (control, index) in Children.OfType<TableCellControl>().Indexed())
        {
            control.Render(canvas, parentSize, cultureInfo);
            var width = Table.CellWidths[index];
            canvas.Translate(width, 0);
        }
        canvas.PopState();
    }

    /// <inheritdoc />
    public override bool CanAdd(Type type) => type.IsEquivalentTo(typeof(TableCellControl));
}