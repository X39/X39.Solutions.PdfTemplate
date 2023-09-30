using System.Globalization;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Attributes;
using X39.Solutions.PdfTemplate.Controls.Base;
using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate.Controls;

/// <summary>
/// A control which will draw a table.
/// </summary>
[Control(Constants.ControlsNamespace)]
public sealed class TableControl : AlignableContentControl
{
    internal readonly Dictionary<int, float> CellWidths = new();

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

        var totalCellWidth = CellWidths.Values.Sum();
        var factor = remainingSize.Width / totalCellWidth;
        foreach (var (index, width) in CellWidths)
        {
            CellWidths[index] = width * factor;
        }


        return new Size(maxWidth, totalHeight);
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