using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Canvas;
using X39.Solutions.Papercraft.Controls.Base;
using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.Papercraft.Controls;

/// <summary>
/// Block-level multi-column container that flows whole child controls across columns.
/// </summary>
[Control(Constants.ControlsNamespace, "columns")]
public sealed class ColumnsControl : AlignableContentControl
{
    private readonly List<ArrangedChild> _arrangedChildren = new();
    private readonly List<ColumnSet> _columnSets = new();
    private int _count = 2;

    /// <summary>
    /// The number of columns used for each column set.
    /// </summary>
    [Parameter(Name = "count")]
    public int ColumnCount
    {
        get => _count;
        set => _count = Math.Max(1, value);
    }

    /// <summary>
    /// The horizontal gap between columns.
    /// </summary>
    [Parameter]
    public Length Gap { get; set; } = new(5F, ELengthUnit.Millimeters);

    /// <summary>
    /// The thickness of vertical rules drawn between columns.
    /// </summary>
    [Parameter]
    public Length RuleThickness { get; set; } = new(0F, ELengthUnit.Pixel);

    /// <summary>
    /// The color of vertical rules drawn between columns.
    /// </summary>
    [Parameter]
    public Color RuleColor { get; set; } = Colors.Transparent;

    [Parameter(Name = "rule-thickness")]
    private Length RuleThicknessHyphen
    {
        get => RuleThickness;
        set => RuleThickness = value;
    }

    [Parameter(Name = "rule-color")]
    private Color RuleColorHyphen
    {
        get => RuleColor;
        set => RuleColor = value;
    }

    /// <inheritdoc />
    public override bool CanAdd(Type type) => true;

    /// <inheritdoc />
    protected override Size DoMeasure(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
        => MeasureOrArrange(dpi, fullPageSize, remainingSize, cultureInfo, arrange: false);

    /// <inheritdoc />
    protected override Size DoArrange(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        _arrangedChildren.Clear();
        _columnSets.Clear();
        return MeasureOrArrange(dpi, fullPageSize, remainingSize, cultureInfo, arrange: true);
    }

    /// <inheritdoc />
    protected override Size PreRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        var baseAdditionalSize = base.PreRender(canvas, dpi, parentSize, cultureInfo);
        if (!Clip)
            return baseAdditionalSize;

        var dryRunCanvas = DryRunDeferredCanvas.From(canvas);
        dryRunCanvas.Translate(ArrangementInner);
        var contentAdditionalSize = RenderArrangedChildren(dryRunCanvas, dpi, cultureInfo);

        return new Size(
            Math.Max(baseAdditionalSize.Width, contentAdditionalSize.Width),
            baseAdditionalSize.Height + contentAdditionalSize.Height);
    }

    /// <inheritdoc />
    protected override Size DoRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        RenderRules(canvas, dpi);
        return RenderArrangedChildren(canvas, dpi, cultureInfo);
    }

    private Size RenderArrangedChildren(IDeferredCanvas canvas, float dpi, CultureInfo cultureInfo)
    {
        var additionalWidth = 0F;
        var additionalHeight = 0F;
        foreach (var arrangedChild in _arrangedChildren)
        {
            Size childAdditionalSize;
            using (canvas.CreateState())
            {
                canvas.Translate(arrangedChild.X, arrangedChild.Y);
                childAdditionalSize = arrangedChild.Control.Render(
                    canvas,
                    dpi,
                    arrangedChild.ParentSize,
                    cultureInfo);
            }

            additionalWidth = Math.Max(
                additionalWidth,
                Math.Max(0F, arrangedChild.X + arrangedChild.Size.Width + childAdditionalSize.Width - ArrangementInner.Width));
            additionalHeight = Math.Max(
                additionalHeight,
                Math.Max(0F, arrangedChild.Y + arrangedChild.Size.Height + childAdditionalSize.Height - ArrangementInner.Height));
        }

        return new Size(additionalWidth, additionalHeight);
    }

    private Size MeasureOrArrange(
        float dpi,
        Size fullPageSize,
        Size remainingSize,
        CultureInfo cultureInfo,
        bool arrange)
    {
        var layout = CalculateLayout(dpi, remainingSize);
        var columnSize = new Size(layout.ColumnWidth, layout.Height);
        var flow = new FlowState(layout.Count);

        foreach (var child in Children)
        {
            var childSize = arrange
                ? child.Arrange(dpi, fullPageSize, columnSize, columnSize, cultureInfo)
                : child.Measure(dpi, fullPageSize, columnSize, columnSize, cultureInfo);

            if (flow.ShouldMoveToNextColumn(childSize.Height, layout.Height))
                flow.MoveToNextColumn(layout.Height);

            if (arrange)
            {
                _arrangedChildren.Add(
                    new ArrangedChild(
                        child,
                        childSize,
                        flow.ColumnX(layout.ColumnWidth, layout.Gap),
                        flow.ChildY,
                        columnSize));
            }

            flow.AddChild(childSize.Height);
        }

        var totalHeight = flow.TotalHeight;
        if (arrange)
            _columnSets.AddRange(flow.GetColumnSets());

        return new Size(remainingSize.Width, totalHeight);
    }

    private Layout CalculateLayout(float dpi, Size remainingSize)
    {
        var count = ColumnCount;
        var gap = Math.Max(0F, Gap.ToPixels(remainingSize.Width, dpi));
        var totalGap = gap * (count - 1);
        var columnWidth = Math.Max(0F, remainingSize.Width - totalGap) / count;
        return new Layout(count, gap, columnWidth, Math.Max(0F, remainingSize.Height));
    }

    private void RenderRules(IDeferredCanvas canvas, float dpi)
    {
        var thickness = Math.Max(0F, RuleThickness.ToPixels(ArrangementInner.Width, dpi));
        if (thickness <= 0F || RuleColor.Alpha is 0 || ColumnCount <= 1)
            return;

        var layout = CalculateLayout(dpi, ArrangementInner);
        foreach (var columnSet in _columnSets.Where((q) => q.Height > 0F))
        {
            for (var column = 1; column < layout.Count; column++)
            {
                var x = column * (layout.ColumnWidth + layout.Gap) - layout.Gap / 2F;
                canvas.DrawLine(
                    RuleColor,
                    thickness,
                    x,
                    columnSet.Y,
                    x,
                    columnSet.Y + columnSet.Height);
            }
        }
    }

    private readonly record struct Layout(int Count, float Gap, float ColumnWidth, float Height);

    private readonly record struct ArrangedChild(
        IControl Control,
        Size Size,
        float X,
        float Y,
        Size ParentSize);

    private readonly record struct ColumnSet(float Y, float Height);

    private sealed class FlowState
    {
        private readonly float[] _columnHeights;
        private readonly bool[] _columnHasContent;
        private readonly List<ColumnSet> _columnSets = new();
        private float _setStartY;
        private int _columnIndex;

        public FlowState(int count)
        {
            _columnHeights = new float[count];
            _columnHasContent = new bool[count];
        }

        public float ChildY => _setStartY + _columnHeights[_columnIndex];

        public float TotalHeight => _setStartY + CurrentSetHeight;

        private float CurrentSetHeight => _columnHeights.DefaultIfEmpty().Max();

        public bool ShouldMoveToNextColumn(float childHeight, float columnHeight)
            => _columnHasContent[_columnIndex]
               && _columnHeights[_columnIndex] + childHeight > columnHeight;

        public void MoveToNextColumn(float columnHeight)
        {
            if (_columnIndex < _columnHeights.Length - 1)
            {
                _columnIndex++;
                return;
            }

            FinishCurrentSet(columnHeight);
            _columnIndex = 0;
            Array.Clear(_columnHeights);
            Array.Clear(_columnHasContent);
        }

        public void AddChild(float childHeight)
        {
            _columnHeights[_columnIndex] += childHeight;
            _columnHasContent[_columnIndex] = true;
        }

        public float ColumnX(float columnWidth, float gap)
            => _columnIndex * (columnWidth + gap);

        public IEnumerable<ColumnSet> GetColumnSets()
        {
            FinishCurrentSet(0F);
            return _columnSets;
        }

        private void FinishCurrentSet(float minimumHeight)
        {
            if (!_columnHasContent.Any((q) => q))
                return;

            var height = Math.Max(CurrentSetHeight, minimumHeight);
            _columnSets.Add(new ColumnSet(_setStartY, height));
            _setStartY += height;
        }
    }
}
