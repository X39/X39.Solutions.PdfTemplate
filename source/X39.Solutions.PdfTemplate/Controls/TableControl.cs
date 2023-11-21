using System.Collections.Immutable;
using System.ComponentModel;
using System.Globalization;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Attributes;
using X39.Solutions.PdfTemplate.Controls.Base;
using X39.Solutions.PdfTemplate.Data;
using X39.Util.Collections;

namespace X39.Solutions.PdfTemplate.Controls;

/// <summary>
/// A control which will draw a table.
/// </summary>
[Control(Constants.ControlsNamespace)]
public sealed class TableControl : AlignableContentControl
{
    internal readonly Dictionary<int, (float desiredWitdth, ColumnLength columnLength)> CellWidths = new();

    /// <inheritdoc />
    public override void Add(IControl item)
    {
        if (item is not TableRowControlBase tableRowControl)
            throw new ArgumentException("Only TableRowControl can be added to a TableControl");
        if (tableRowControl.Table is not null)
            throw new InvalidOperationException("A TableRowControl can only be added to one TableControl");
        tableRowControl.Table = this;
        base.Add(item);
    }

    /// <inheritdoc />
    public override bool Remove(IControl item)
    {
        if (item is not TableRowControlBase tableRowControl)
            throw new ArgumentException("Only TableRowControl can be added to a TableControl");
        if (!base.Remove(item))
            return false;
        if (tableRowControl.Table != this)
            throw new InvalidOperationException(
                "A TableRowControl can only be removed from the TableControl it was added to");
        tableRowControl.Table = null;
        return true;
    }

    /// <inheritdoc />
    protected override Size DoMeasure(
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        var totalHeaderHeight = 0F;
        var maxWidth = 0F;
        var totalHeight = 0F;
        foreach (var control in Children.OfType<TableHeaderControl>())
        {
            var size = control.Measure(fullPageSize, framedPageSize, remainingSize, cultureInfo);
            totalHeaderHeight += size.Height;
            maxWidth          =  Math.Max(maxWidth, size.Width);
        }

        var currentHeight = remainingSize.Height;
        foreach (var control in Children.OfType<TableRowControl>())
        {
            var size = control.Measure(fullPageSize, framedPageSize, remainingSize, cultureInfo);
            maxWidth = Math.Max(maxWidth, size.Width);
            if (currentHeight + size.Height >= remainingSize.Height)
            {
                totalHeight   += remainingSize.Height - currentHeight;
                totalHeight   += totalHeaderHeight;
                currentHeight =  totalHeaderHeight;
            }

            currentHeight += size.Height;
            totalHeight   += size.Height;
        }

        var desiredTotalWidth =
            HorizontalAlignment is EHorizontalAlignment.Stretch
                ? framedPageSize.Width
                : CellWidths.Values.Select((q) => q.desiredWitdth).Sum();

        var outWidths = CalculateWidths(
            desiredTotalWidth,
            CellWidths.Values
                .Select((q) => (q.columnLength, q.desiredWitdth))
                .ToArray());
        for (var i = 0; i < CellWidths.Count; i++)
        {
            var key = CellWidths.Keys.ElementAt(i);
            var (_, columnLength) = CellWidths[key];
            var newValue = (outWidths.ElementAt(i), columnLength);
            CellWidths[key] = newValue;
        }

        return new Size(maxWidth, totalHeight);
    }

    private static IReadOnlyCollection<float> CalculateWidths(
        float totalWidth,
        IReadOnlyCollection<(ColumnLength length, float desiredWidth)> columns)
    {
        var totalPixelWidth = columns
            .Where((q) => q.length.Unit == EColumnUnit.Pixel)
            .Select((q) => q.length.Value)
            .DefaultIfEmpty()
            .Sum();
        var totalPercentWidth = columns
            .Where((q) => q.length.Unit == EColumnUnit.Percent)
            .Select((q) => q.length.Value * totalWidth)
            .DefaultIfEmpty()
            .Sum();

        var remainingWidth = totalWidth;
        remainingWidth -= totalPixelWidth;
        remainingWidth -= totalPercentWidth;

        var totalParts = columns
            .Where((q) => q.length.Unit == EColumnUnit.Part)
            .Select((q) => q.length.Value)
            .DefaultIfEmpty()
            .Sum();
        var totalAutoWidth = columns
            .Where((q) => q.length.Unit == EColumnUnit.Auto)
            .Select((q) => q.desiredWidth)
            .Sum();

        var autoCoef = remainingWidth < totalAutoWidth
            ? remainingWidth / totalAutoWidth
            : 1.0F;
        if (totalParts > 0)
        {
            remainingWidth -= totalAutoWidth;
            var partWidth = remainingWidth / totalParts;
            var outWidths = new float[columns.Count];
            foreach (var ((length, desiredWidth), index) in columns.Indexed())
            {
                outWidths[index] = length.Unit switch
                {
                    EColumnUnit.Auto    => desiredWidth * autoCoef,
                    EColumnUnit.Pixel   => length.Value,
                    EColumnUnit.Part    => partWidth * length.Value,
                    EColumnUnit.Percent => totalWidth * length.Value,
                    _ => throw new InvalidEnumArgumentException(
                        nameof(length.Unit),
                        (int) length.Unit,
                        typeof(EColumnUnit)),
                };
            }

            return outWidths.ToImmutableArray();
        }
        else
        {
            var max = totalWidth / columns.Count;
            var larger = columns
                .Where(x => x.length.Unit == EColumnUnit.Auto)
                .Where(x => x.desiredWidth > max)
                .DefaultIfEmpty()
                .Sum(x => x.desiredWidth);
            var remainingWidthWithoutLarger = remainingWidth - larger;
            var remainingCount = columns
                .Where(x => x.length.Unit == EColumnUnit.Auto)
                .Count(x => x.desiredWidth <= max);
            var newWidth = remainingWidthWithoutLarger / remainingCount;
            var outWidths = new float[columns.Count];
            foreach (var ((length, desiredWidth), index) in columns.Indexed())
            {
                outWidths[index] = length.Unit switch
                {
                    EColumnUnit.Auto => desiredWidth > max || autoCoef < 1.0F
                        ? desiredWidth * autoCoef
                        : newWidth,
                    EColumnUnit.Pixel   => length.Value,
                    EColumnUnit.Part    => 0F, // We don't have any parts in this branch
                    EColumnUnit.Percent => totalWidth * length.Value,
                    _ => throw new InvalidEnumArgumentException(
                        nameof(length.Unit),
                        (int) length.Unit,
                        typeof(EColumnUnit)),
                };
            }

            return outWidths.ToImmutableArray();
        }
    }

    /// <inheritdoc />
    protected override Size DoArrange(
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        var totalHeaderHeight = 0F;
        var maxWidth = 0F;
        var totalHeight = 0F;
        foreach (var control in Children.OfType<TableHeaderControl>())
        {
            var size = control.Arrange(fullPageSize, framedPageSize, remainingSize, cultureInfo);
            totalHeaderHeight += size.Height;
            maxWidth          =  Math.Max(maxWidth, size.Width);
        }

        var currentHeight = remainingSize.Height;
        foreach (var control in Children.OfType<TableRowControl>())
        {
            var size = control.Arrange(fullPageSize, framedPageSize, remainingSize, cultureInfo);
            maxWidth = Math.Max(maxWidth, size.Width);
            if (currentHeight + size.Height >= remainingSize.Height)
            {
                totalHeight   += remainingSize.Height - currentHeight;
                totalHeight   += totalHeaderHeight;
                currentHeight =  totalHeaderHeight;
            }

            currentHeight += size.Height;
            totalHeight   += size.Height;
        }

        return new Size(maxWidth, totalHeight);
    }

    /// <inheritdoc />
    protected override void DoRender(ICanvas canvas, in Size parentSize, CultureInfo cultureInfo)
    {
        var height = 0F;
        var headers = Children.OfType<TableHeaderControl>().ToArray();
        foreach (var headerControl in headers)
        {
            headerControl.Render(canvas, parentSize, cultureInfo);
            canvas.Translate(0, headerControl.Arrangement.Height);
            height += headerControl.Arrangement.Height;
        }

        foreach (var control in Children.OfType<TableRowControl>())
        {
            if (height + control.Arrangement.Height > parentSize.Height)
            {
                var delta = parentSize.Height - height;
                canvas.Translate(0, delta);
                height = 0F;
                foreach (var headerControl in headers)
                {
                    headerControl.Render(canvas, parentSize, cultureInfo);
                    canvas.Translate(0, headerControl.Arrangement.Height);
                    height += headerControl.Arrangement.Height;
                }
            }

            control.Render(canvas, parentSize, cultureInfo);
            canvas.Translate(0, control.Arrangement.Height);
            height += control.Arrangement.Height;
        }
    }

    /// <inheritdoc />
    public override bool CanAdd(Type type)
    {
        return type.IsEquivalentTo(typeof(TableRowControl)) || type.IsEquivalentTo(typeof(TableHeaderControl));
    }
}