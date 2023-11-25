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
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        if (Table is null)
            throw new InvalidOperationException("A TableRowControl must be added to a TableControl");
        var width = 0F;
        var height = 0F;
        foreach (var (control, index) in Children.Cast<TableCellControl>().Indexed())
        {
            var size = control.Measure(dpi, fullPageSize, remainingSize, remainingSize, cultureInfo);
            width              += size.Width;
            height             =  Math.Max(height, size.Height);
            if (Table.CellWidths.TryGetValue(index, out var tuple))
            {
                var (cellWidth, columnLength) = tuple;
                var desiredWidth = Math.Max(size.Width, cellWidth);
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (desiredWidth != cellWidth) // We explicitly want to check for equality here.
                    Table.CellWidths[index] = (desiredWidth, columnLength);
            }
            else
            {
                var desiredWidth = size.Width;
                Table.CellWidths[index] = (desiredWidth, control.Width);
            }
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
        if (Table is null)
            throw new InvalidOperationException("A TableRowControl must be added to a TableControl");
        var width = 0F;
        var height = 0F;
        foreach (var (control, index) in Children.Indexed())
        {
            _                  = Table.CellWidths.TryGetValue(index, out var tuple);
            var (cellWidth, _) = tuple;
            var remainingCellSize = remainingSize with
            {
                Width = cellWidth,
                Height = MeasurementInner.Height,
            };
            var size = control.Arrange(dpi, fullPageSize, remainingCellSize, remainingCellSize, cultureInfo);
            width  += cellWidth;
            height =  Math.Max(height, size.Height);
        }

        return new Size(width, height);
    }

    /// <inheritdoc />
    protected override void DoRender(ICanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        if (Table is null)
            throw new InvalidOperationException("A TableRowControl must be added to a TableControl");
        canvas.PushState();
        foreach (var (control, index) in Children.OfType<TableCellControl>().Indexed())
        {
            control.Render(canvas, dpi, parentSize, cultureInfo);
            var (width, _) = Table.CellWidths[index];
            canvas.Translate(width, 0);
        }

        canvas.PopState();
    }

    /// <inheritdoc />
    public override bool CanAdd(Type type) => type.IsEquivalentTo(typeof(TableCellControl));
}