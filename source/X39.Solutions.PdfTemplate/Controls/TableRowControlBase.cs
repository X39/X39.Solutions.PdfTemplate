using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Controls.Base;
using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate.Controls;

/// <summary>
/// Base class for table row controls.
/// </summary>
public abstract class TableRowControlBase : AlignableContentControl
{
    internal TableControl? Table { get; set; }

    /// <inheritdoc />
    protected override Size DoMeasure(
        float       dpi,
        in Size     fullPageSize,
        in Size     framedPageSize,
        in Size     remainingSize,
        CultureInfo cultureInfo
    ) => MeasureWithCellWidthOverriding(dpi, fullPageSize, framedPageSize, remainingSize, cultureInfo);

    private Size MeasureWithCellWidthOverriding(
        float       dpi,
        Size        fullPageSize,
        Size        framedPageSize,
        Size        remainingSize,
        CultureInfo cultureInfo
    )
    {
        if (Table?.CellWidths is not { } cellWidths)
            throw new InvalidOperationException(
                "A TableRowControl must be added to a TableControl with a valid CellWidths dictionary"
            );
        ushort cellIndex  = 0;
        var    maxHeight  = 0F;
        var    totalWidth = 0F;
        foreach (var child in Children.OfType<TableCellControl>().Where((cell) => cell.ColumnSpan > 0))
        {
            var size                 = child.Measure(dpi, fullPageSize, framedPageSize, remainingSize, cultureInfo);
            var adjustedCellWidth    = child.Width / child.ColumnSpan;
            var adjustedDesiredWidth = size.Width / child.ColumnSpan;
            for (var localCellIndex = cellIndex; localCellIndex < cellIndex + child.ColumnSpan; localCellIndex++)
            {
                if (!cellWidths.TryGetValue(localCellIndex, out var tuple))
                    tuple = (size.Width, child.Width);
                cellWidths[localCellIndex] = (Math.Max(tuple.desiredWitdth, adjustedDesiredWidth),
                                              tuple.columnLength < adjustedCellWidth
                                                  ? adjustedCellWidth
                                                  : tuple.columnLength);
            }

            cellIndex  += child.ColumnSpan;
            totalWidth += size.Width;
            maxHeight  =  Math.Max(maxHeight, size.Height);
        }

        return new Size(totalWidth, maxHeight);
    }

    internal Size MeasureWithCellWidth(
        float       dpi,
        Size        fullPageSize,
        Size        framedPageSize,
        Size        remainingSize,
        CultureInfo cultureInfo
    )
    {
        if (Table?.CellWidths is not { } cellWidths)
            throw new InvalidOperationException(
                "A TableRowControl must be added to a TableControl with a valid CellWidths dictionary"
            );
        ushort cellIndex  = 0;
        var    maxHeight  = 0F;
        var    totalWidth = 0F;
        foreach (var child in Children.OfType<TableCellControl>().Where((cell) => cell.ColumnSpan > 0))
        {
            var availableWidth = 0F;
            for (var localCellIndex = cellIndex; localCellIndex < cellIndex + child.ColumnSpan; localCellIndex++)
            {
                var (width, _) = cellWidths[localCellIndex];
                availableWidth += width;
            }
            var size                 = child.Measure(dpi, fullPageSize, framedPageSize, remainingSize with
            {
                Width = availableWidth
            }, cultureInfo);
            cellIndex  += child.ColumnSpan;
            totalWidth += size.Width;
            maxHeight  =  Math.Max(maxHeight, size.Height);
        }

        return new Size(totalWidth, maxHeight);
    }

    /// <inheritdoc />
    protected override Size DoArrange(
        float       dpi,
        in Size     fullPageSize,
        in Size     framedPageSize,
        in Size     remainingSize,
        CultureInfo cultureInfo
    )
    {
        if (Table?.CellWidths is not { } cellWidths)
            throw new InvalidOperationException(
                "A TableRowControl must be added to a TableControl with a valid CellWidths dictionary"
            );
        var   maxHeight = MeasurementInner.Height;
        float previousMaxHeight;
        var   count = 0;
        do
        {
            previousMaxHeight = maxHeight;
            ushort cellIndex  = 0;
            foreach (var child in Children.OfType<TableCellControl>().Where((cell) => cell.ColumnSpan > 0))
            {
                var widthAvailable = 0F;
                for (var localCellIndex = cellIndex; localCellIndex < cellIndex + child.ColumnSpan; localCellIndex++)
                {
                    _              =  cellWidths.TryGetValue(localCellIndex, out var tuple);
                    widthAvailable += tuple.desiredWitdth;
                }

                cellIndex += child.ColumnSpan;

                var size = child.Arrange(
                    dpi,
                    fullPageSize,
                    new Size(Width: widthAvailable, Height: maxHeight),
                    new Size(Width: widthAvailable, Height: maxHeight),
                    cultureInfo
                );
                maxHeight = Math.Max(maxHeight, size.Height);
            }
            // ReSharper disable once CompareOfFloatsByEqualityOperator
        } while (previousMaxHeight != maxHeight && ++count < 2);

        return remainingSize with { Height = maxHeight };
    }

    /// <inheritdoc />
    protected override Size DoRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        var additionalWidth  = 0F;
        var additionalHeight = 0F;
        if (Table is null)
            throw new InvalidOperationException("A TableRowControl must be added to a TableControl");
        using (canvas.CreateState())
        {
            ushort localCellIndex = 0;
            foreach (var control in Children.OfType<TableCellControl>().Where((cell) => cell.ColumnSpan > 0))
            {
                var widthAvailable = 0F;
                for (var i = localCellIndex; i < localCellIndex + control.ColumnSpan; i++)
                {
                    _              =  Table.CellWidths.TryGetValue(i, out var tuple);
                    widthAvailable += tuple.desiredWitdth;
                }

                localCellIndex      += control.ColumnSpan;
                var (width, height) =  control.Render(canvas, dpi, parentSize, cultureInfo);
                additionalWidth     += width;
                additionalHeight    += height;
                canvas.Translate(widthAvailable, 0);
            }
        }

        return new Size(additionalWidth, additionalHeight);
    }

    /// <inheritdoc />
    public override bool CanAdd(Type type) => type.IsEquivalentTo(typeof(TableCellControl));
}
