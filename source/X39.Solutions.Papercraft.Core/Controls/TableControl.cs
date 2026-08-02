using System.ComponentModel;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Canvas;
using X39.Solutions.Papercraft.Controls.Base;
using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.Papercraft.Controls;

/// <summary>
/// A control which will draw a table.
/// </summary>
[Control(Constants.ControlsNamespace)]
public sealed class TableControl : AlignableContentControl
{
    private const float PageBoundaryTolerance = 0.001F;

    internal sealed class CellWidthsDictionary<T>
    {
        private readonly List<T?> _backingList = new();

        public IReadOnlyCollection<T?> Values
            => _backingList.AsReadOnly();
        
        public ushort Count => (ushort)_backingList.Count;

        public void Reset()
        {
            _backingList.Clear();
        }
        
        internal T? this[ushort index]
        {
            get => _backingList.Count <= index ? default : _backingList[index];
            set
            {
                if (_backingList.Count <= index)
                    _backingList.AddRange(Enumerable.Repeat(default(T), index - _backingList.Count + 1));
                _backingList[index] = value;
            }
        }

        public bool TryGetValue(ushort localCellIndex, out T t)
        {
            if (_backingList.Count <= localCellIndex)
            {
                t = default!;
                return false;
            }

            t = _backingList[localCellIndex]!;
            return true;
        }

        public void ApplyForEach(Func<ushort, T, T> func)
        {
            for (ushort i = 0; i < _backingList.Count; i++)
            {
                _backingList[i] = func(i, _backingList[i]!);
            }
        }
    }
    internal readonly CellWidthsDictionary<(float desiredWitdth, ColumnLength columnLength)> CellWidths = new();

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
                "A TableRowControl can only be removed from the TableControl it was added to"
            );
        tableRowControl.Table = null;
        return true;
    }

    /// <inheritdoc />
    protected override Size DoMeasure(
        float       dpi,
        in Size     fullPageSize,
        in Size     framedPageSize,
        in Size     remainingSize,
        CultureInfo cultureInfo
    )
    {
        // Must be called as this contains the measure width of each individual cell.
        CellWidths.Reset();
        var totalHeaderHeight = 0F;
        var maxWidth          = 0F;
        var totalHeight       = 0F;
        foreach (var control in Children.OfType<TableHeaderControl>())
        {
            var size = control.Measure(dpi, fullPageSize, remainingSize, remainingSize, cultureInfo);
            totalHeaderHeight += size.Height;
            maxWidth          =  Math.Max(maxWidth, size.Width);
        }

        var currentHeight = remainingSize.Height;
        foreach (var control in Children.OfType<TableRowControl>())
        {
            var size = control.Measure(dpi, fullPageSize, remainingSize, remainingSize, cultureInfo);
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

        var desiredTotalWidth = HorizontalAlignment is EHorizontalAlignment.Stretch ? framedPageSize.Width : maxWidth;

        var outWidths = CalculateWidths(
            desiredTotalWidth,
            dpi,
            CellWidths.Values.Select((q) => (q.columnLength, q.desiredWitdth)).ToArray()
        );
        for (ushort i = 0; i < CellWidths.Count; i++)
        {
            var (_, columnLength) = CellWidths[i];
            var newValue = (outWidths.ElementAt(i), columnLength);
            CellWidths[i] = newValue;
        }

        return new Size(desiredTotalWidth, totalHeight);
    }

    private static IReadOnlyCollection<float> CalculateWidths(
        float                                                          totalWidth,
        float                                                          dpi,
        IReadOnlyCollection<(ColumnLength length, float desiredWidth)> columns
    )
    {
        var totalFixedWidth = columns
                              .Where(
                                  (q) => q.length.Unit == EColumnUnit.Length
                                         && q.length.Length?.Unit != ELengthUnit.Auto
                              )
                              .Select((q) => q.length.Length?.ToPixels(totalWidth, dpi))
                              .Where((q) => q.HasValue)
                              .Select((q) => q.GetValueOrDefault())
                              .DefaultIfEmpty()
                              .Sum();

        var remainingWidth = totalWidth;
        remainingWidth -= totalFixedWidth;

        var totalParts = columns.Where((q) => q.length.Unit == EColumnUnit.Parts)
                                .Select((q) => q.length.Value)
                                .DefaultIfEmpty()
                                .Sum();
        var totalAutoWidth = columns
                             .Where((q) => q.length is { Unit: EColumnUnit.Length, Length.Unit: ELengthUnit.Auto })
                             .Select((q) => q.desiredWidth)
                             .Sum();

        var autoCoef = remainingWidth < totalAutoWidth ? remainingWidth / totalAutoWidth : 1.0F;
        if (totalParts > 0)
        {
            remainingWidth -= totalAutoWidth;
            var partWidth = remainingWidth / totalParts;
            var outWidths = new float[columns.Count];
            foreach (var ((length, desiredWidth), index) in columns.Select((column, index) => (column, index)))
            {
                outWidths[index] = length.Unit switch
                {
                    EColumnUnit.Parts => partWidth * length.Value ?? 0F,
                    EColumnUnit.Length => length.Length?.Unit switch
                    {
                        ELengthUnit.Auto => desiredWidth * autoCoef,
                        _                => length.Length?.ToPixels(totalWidth, dpi) ?? 0F,
                    },
                    _ => throw new InvalidEnumArgumentException(
                        nameof(length.Unit),
                        (int) length.Unit,
                        typeof(EColumnUnit)
                    ),
                };
            }

            return outWidths;
        }
        else
        {
            var max = totalWidth / columns.Count;
            var larger = columns.Where(x => x.length is { Unit: EColumnUnit.Length, Length.Unit: ELengthUnit.Auto })
                                .Where(x => x.desiredWidth > max)
                                .DefaultIfEmpty()
                                .Sum(x => x.desiredWidth);
            var remainingWidthWithoutLarger = remainingWidth - larger;
            var remainingCount = columns
                                 .Where(x => x.length is { Unit: EColumnUnit.Length, Length.Unit: ELengthUnit.Auto })
                                 .Count(x => x.desiredWidth <= max);
            var newWidth  = remainingWidthWithoutLarger / remainingCount;
            var outWidths = new float[columns.Count];
            foreach (var ((length, desiredWidth), index) in columns.Select((column, index) => (column, index)))
            {
                outWidths[index] = length.Unit switch
                {
                    EColumnUnit.Parts => 0F, // We don't have any parts in this branch
                    EColumnUnit.Length => length.Length?.Unit switch
                    {
                        ELengthUnit.Auto => desiredWidth > max || autoCoef < 1.0F ? desiredWidth * autoCoef : newWidth,
                        _                => length.Length?.ToPixels(totalWidth, dpi) ?? 0F,
                    },
                    _ => throw new InvalidEnumArgumentException(
                        nameof(length.Unit),
                        (int) length.Unit,
                        typeof(EColumnUnit)
                    ),
                };
            }

            return outWidths;
        }
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
        var totalHeight       = 0F;
        var maxWidth          = 0F;
        foreach (var tableRow in Children.OfType<TableHeaderControl>())
        {
            // Measure again to make the correct width is used.
            tableRow.MeasureWithCellWidth(dpi, fullPageSize, framedPageSize, remainingSize, cultureInfo);
            var arrangeSize = tableRow.Arrange(dpi, fullPageSize, framedPageSize, remainingSize, cultureInfo);
            maxWidth = Math.Max(maxWidth, arrangeSize.Width);
            totalHeight += arrangeSize.Height;
        }

        foreach (var tableRow in Children.OfType<TableRowControl>())
        {
            // Measure again to make the correct width is used.
            tableRow.MeasureWithCellWidth(dpi, fullPageSize, framedPageSize, remainingSize, cultureInfo);
            var arrangeSize = tableRow.Arrange(dpi, fullPageSize, framedPageSize, remainingSize, cultureInfo);
            maxWidth = Math.Max(maxWidth, arrangeSize.Width);
            totalHeight += arrangeSize.Height;
        }

        return new Size(maxWidth, totalHeight);
    }

    /// <inheritdoc />
    protected override Size PreRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        var dryRunCanvas = DryRunDeferredCanvas.From(canvas);
        dryRunCanvas.Translate(ArrangementInner);
        return RenderTable(dryRunCanvas, dpi, parentSize, cultureInfo, renderControls: true);
    }

    /// <inheritdoc />
    protected override Size DoRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        return RenderTable(canvas, dpi, parentSize, cultureInfo, renderControls: true);
    }

    private Size RenderTable(
        IDeferredCanvas canvas,
        float dpi,
        in Size parentSize,
        CultureInfo cultureInfo,
        bool renderControls)
    {
        var additionalWidth  = 0F;
        var additionalHeight = 0F;
        var headers          = Children.OfType<TableHeaderControl>().ToArray();
        var rows             = Children.OfType<TableRowControl>().ToArray();
        var pageHeight       = canvas.PageSize.Height > 0F ? canvas.PageSize.Height : parentSize.Height;

        if (ShouldMoveInitialHeadersWithFirstRow(
                canvas,
                dpi,
                parentSize,
                pageHeight,
                cultureInfo,
                headers,
                rows.FirstOrDefault(),
                renderControls))
        {
            var remainingPageHeight = canvas.GetRemainingPageHeight(pageHeight);
            canvas.Translate(0, remainingPageHeight);
            additionalHeight += remainingPageHeight;
        }

        var (width, height) = RenderInitialHeaders(canvas, dpi, parentSize, cultureInfo, headers, renderControls);
        additionalWidth  += width;
        additionalHeight += height;

        foreach (var control in rows)
        {
            if (IsAtStartOfPage(canvas, pageHeight))
            {
                if (CanRenderRepeatedHeaders(headers, control, pageHeight))
                {
                    (width, height) = RenderRepeatedHeaders(canvas, dpi, parentSize, cultureInfo, headers, renderControls);
                    additionalWidth  += width;
                    additionalHeight += height;
                }
            }
            else
            {
                var remainingPageHeight = canvas.GetRemainingPageHeight(pageHeight);
                var rowAdditionalSize = MeasureRenderAdditionalSize(
                    control,
                    canvas,
                    dpi,
                    parentSize,
                    cultureInfo,
                    renderControls);
                var rowRenderHeight = control.ArrangementOuter.Height + rowAdditionalSize.Height;
                if (rowRenderHeight > remainingPageHeight + PageBoundaryTolerance)
                {
                    canvas.Translate(0, remainingPageHeight);
                    additionalHeight += remainingPageHeight;
                    if (CanRenderRepeatedHeaders(headers, control, pageHeight))
                    {
                        (width, height) = RenderRepeatedHeaders(canvas, dpi, parentSize, cultureInfo, headers, renderControls);
                        additionalWidth  += width;
                        additionalHeight += height;
                    }
                }
            }

            (width, height) = RenderControl(control, canvas, dpi, parentSize, cultureInfo, renderControls);
            additionalWidth  += width;
            additionalHeight += height;
            canvas.Translate(0, control.ArrangementOuter.Height + height);
        }

        return new Size(additionalWidth, additionalHeight);
    }

    private static bool ShouldMoveInitialHeadersWithFirstRow(
        IDeferredCanvas canvas,
        float dpi,
        in Size parentSize,
        float pageHeight,
        CultureInfo cultureInfo,
        IReadOnlyCollection<TableHeaderControl> headers,
        TableRowControl? firstRow,
        bool renderControls)
    {
        if (headers.Count is 0 || firstRow is null)
            return false;

        var usedPageHeight = canvas.GetUsedPageHeight(pageHeight);
        if (usedPageHeight <= PageBoundaryTolerance)
            return false;

        var dryRunCanvas = DryRunDeferredCanvas.From(canvas);
        var headersHeight = RenderHeadersAndMeasureHeight(
            dryRunCanvas,
            dpi,
            parentSize,
            cultureInfo,
            headers,
            renderControls);
        var rowAdditionalSize = MeasureRenderAdditionalSize(
            firstRow,
            dryRunCanvas,
            dpi,
            parentSize,
            cultureInfo,
            renderControls);
        var requiredHeight = headersHeight + firstRow.ArrangementOuter.Height + rowAdditionalSize.Height;
        var remainingPageHeight = pageHeight - usedPageHeight;
        return requiredHeight > remainingPageHeight + PageBoundaryTolerance;
    }

    private static float RenderHeadersAndMeasureHeight(
        IDeferredCanvas canvas,
        float dpi,
        in Size parentSize,
        CultureInfo cultureInfo,
        IReadOnlyCollection<TableHeaderControl> headers,
        bool renderControls)
    {
        var height = 0F;
        foreach (var headerControl in headers)
        {
            var additionalSize = RenderControl(headerControl, canvas, dpi, parentSize, cultureInfo, renderControls);
            var headerHeight = headerControl.ArrangementOuter.Height + additionalSize.Height;
            height += headerHeight;
            canvas.Translate(0, headerHeight);
        }

        return height;
    }

    private static Size MeasureRenderAdditionalSize(
        IControl control,
        IDeferredCanvas canvas,
        float dpi,
        in Size parentSize,
        CultureInfo cultureInfo,
        bool renderControls)
    {
        if (!renderControls)
            return Size.Zero;

        var dryRunCanvas = DryRunDeferredCanvas.From(canvas);
        return control.Render(dryRunCanvas, dpi, parentSize, cultureInfo);
    }

    private static bool CanRenderRepeatedHeaders(
        IReadOnlyCollection<TableHeaderControl> headers,
        TableRowControl row,
        float pageHeight)
    {
        if (headers.Count is 0)
            return false;

        var repeatedHeaderHeight = headers.Sum((header) => header.ArrangementOuter.Height);
        if (row.ArrangementOuter.Height > pageHeight + PageBoundaryTolerance)
            return true;

        return repeatedHeaderHeight + row.ArrangementOuter.Height <= pageHeight + PageBoundaryTolerance;
    }

    private static bool IsAtStartOfPage(IDeferredCanvas canvas, float pageHeight)
    {
        if (canvas.Translation.Y <= PageBoundaryTolerance)
            return false;

        return canvas.GetUsedPageHeight(pageHeight) <= PageBoundaryTolerance;
    }

    private static Size RenderInitialHeaders(
        IDeferredCanvas canvas,
        float dpi,
        in Size parentSize,
        CultureInfo cultureInfo,
        IReadOnlyCollection<TableHeaderControl> headers,
        bool renderControls)
    {
        var additionalWidth  = 0F;
        var additionalHeight = 0F;
        foreach (var headerControl in headers)
        {
            var (width, height) = RenderControl(headerControl, canvas, dpi, parentSize, cultureInfo, renderControls);
            additionalWidth  += width;
            additionalHeight += height;
            canvas.Translate(0, headerControl.ArrangementOuter.Height + height);
        }

        return new Size(additionalWidth, additionalHeight);
    }

    private static Size RenderRepeatedHeaders(
        IDeferredCanvas canvas,
        float dpi,
        in Size parentSize,
        CultureInfo cultureInfo,
        IReadOnlyCollection<TableHeaderControl> headers,
        bool renderControls)
    {
        var additionalWidth  = 0F;
        var additionalHeight = 0F;
        foreach (var headerControl in headers)
        {
            var (width, height) = RenderControl(headerControl, canvas, dpi, parentSize, cultureInfo, renderControls);
            additionalWidth  += width;
            additionalHeight += headerControl.ArrangementOuter.Height + height;
            canvas.Translate(0, headerControl.ArrangementOuter.Height + height);
        }

        return new Size(additionalWidth, additionalHeight);
    }

    private static Size RenderControl(
        IControl control,
        IDeferredCanvas canvas,
        float dpi,
        in Size parentSize,
        CultureInfo cultureInfo,
        bool renderControls)
    {
        return renderControls
            ? control.Render(canvas, dpi, parentSize, cultureInfo)
            : Size.Zero;
    }

    /// <inheritdoc />
    public override bool CanAdd(Type type)
    {
        return type.IsEquivalentTo(typeof(TableRowControl)) || type.IsEquivalentTo(typeof(TableHeaderControl));
    }
}
