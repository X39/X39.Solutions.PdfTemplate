using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Canvas;
using X39.Solutions.Papercraft.Controls.Base;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Services.TextService;
using PapercraftSize = X39.Solutions.Papercraft.Data.Size;

namespace X39.Solutions.Papercraft.Controls;

/// <summary>
/// Visual checkbox mark with optional label content.
/// </summary>
[Control(Constants.ControlsNamespace, "checkbox")]
public sealed class CheckboxControl : AlignableContentControl
{
    private readonly ITextService _textService;
    private readonly TextStyle _labelTextStyle = new();
    private readonly List<PapercraftSize> _arrangedChildSizes = new();

    /// <summary>
    /// Creates a new instance of <see cref="CheckboxControl"/>.
    /// </summary>
    /// <param name="textService">The text service used to measure and render the label.</param>
    [ControlConstructor]
    public CheckboxControl(ITextService textService)
    {
        _textService = textService;
    }

    /// <summary>
    /// Whether the checkbox should render a check mark.
    /// </summary>
    [Parameter]
    public bool Checked { get; set; }

    /// <summary>
    /// The width and height of the checkbox square.
    /// </summary>
    [Parameter]
    public Length Size { get; set; } = new(4F, ELengthUnit.Millimeters);

    /// <summary>
    /// Optional plain text label. Element text may also populate this value.
    /// </summary>
    [Parameter(IsContent = true)]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Distance between the square and label or child content.
    /// </summary>
    [Parameter]
    public Length Gap { get; set; } = new(2F, ELengthUnit.Millimeters);

    /// <summary>
    /// The checkbox border color.
    /// </summary>
    [Parameter]
    public Color StrokeColor { get; set; } = Colors.Black;

    /// <summary>
    /// The checkbox fill color.
    /// </summary>
    [Parameter]
    public Color Fill { get; set; } = Colors.Transparent;

    /// <summary>
    /// The check mark color.
    /// </summary>
    [Parameter]
    public Color CheckColor { get; set; } = Colors.Black;

    /// <summary>
    /// The border and check mark stroke thickness.
    /// </summary>
    [Parameter]
    public Length StrokeThickness { get; set; } = new(1F, ELengthUnit.Points);

    /// <inheritdoc />
    public override bool CanAdd(Type type) => true;

    /// <inheritdoc />
    protected override PapercraftSize DoMeasure(
        float dpi,
        in PapercraftSize fullPageSize,
        in PapercraftSize framedPageSize,
        in PapercraftSize remainingSize,
        CultureInfo cultureInfo)
        => MeasureOrArrange(dpi, fullPageSize, remainingSize, cultureInfo, arrange: false);

    /// <inheritdoc />
    protected override PapercraftSize DoArrange(
        float dpi,
        in PapercraftSize fullPageSize,
        in PapercraftSize framedPageSize,
        in PapercraftSize remainingSize,
        CultureInfo cultureInfo)
    {
        _arrangedChildSizes.Clear();
        return MeasureOrArrange(dpi, fullPageSize, remainingSize, cultureInfo, arrange: true);
    }

    /// <inheritdoc />
    protected override PapercraftSize PreRender(
        IDeferredCanvas canvas,
        float dpi,
        in PapercraftSize parentSize,
        CultureInfo cultureInfo)
    {
        var baseAdditionalSize = base.PreRender(canvas, dpi, parentSize, cultureInfo);
        if (!Clip || Children.Count is 0 || !string.IsNullOrEmpty(GetLabel()))
            return baseAdditionalSize;

        var dryRunCanvas = DryRunDeferredCanvas.From(canvas);
        dryRunCanvas.Translate(ArrangementInner);
        var contentAdditionalSize = RenderChildren(
            dryRunCanvas,
            dpi,
            parentSize,
            cultureInfo,
            GetBoxSize(dpi, parentSize));

        return new PapercraftSize(
            Math.Max(baseAdditionalSize.Width, contentAdditionalSize.Width),
            baseAdditionalSize.Height + contentAdditionalSize.Height);
    }

    /// <inheritdoc />
    protected override PapercraftSize DoRender(
        IDeferredCanvas canvas,
        float dpi,
        in PapercraftSize parentSize,
        CultureInfo cultureInfo)
    {
        var boxSize = GetBoxSize(dpi, parentSize);
        var strokeThickness = GetStrokeThickness(dpi, boxSize);
        RenderBox(canvas, boxSize, strokeThickness);

        var label = GetLabel();
        if (!string.IsNullOrEmpty(label))
        {
            RenderLabel(canvas, dpi, label, parentSize, boxSize);
            return PapercraftSize.Zero;
        }

        if (Children.Count is 0)
            return PapercraftSize.Zero;

        return RenderChildren(canvas, dpi, parentSize, cultureInfo, boxSize);
    }

    private PapercraftSize MeasureOrArrange(
        float dpi,
        PapercraftSize fullPageSize,
        PapercraftSize remainingSize,
        CultureInfo cultureInfo,
        bool arrange)
    {
        var boxSize = GetBoxSize(dpi, remainingSize);
        var label = GetLabel();
        if (!string.IsNullOrEmpty(label))
        {
            var gap = GetGap(dpi, remainingSize);
            var labelSize = _textService.Measure(
                _labelTextStyle,
                dpi,
                label.AsSpan(),
                GetContentWidth(remainingSize.Width, boxSize, gap));
            return WithContent(boxSize, gap, labelSize);
        }

        if (Children.Count is 0)
            return new PapercraftSize(boxSize, boxSize);

        var childGap = GetGap(dpi, remainingSize);
        var contentSize = new PapercraftSize(
            GetContentWidth(remainingSize.Width, boxSize, childGap),
            remainingSize.Height);
        var childContentSize = MeasureOrArrangeChildren(dpi, fullPageSize, contentSize, cultureInfo, arrange);
        return WithContent(boxSize, childGap, childContentSize);
    }

    private PapercraftSize MeasureOrArrangeChildren(
        float dpi,
        PapercraftSize fullPageSize,
        PapercraftSize contentSize,
        CultureInfo cultureInfo,
        bool arrange)
    {
        var totalHeight = 0F;
        var maxWidth = 0F;
        foreach (var child in Children)
        {
            var childSize = arrange
                ? child.Arrange(dpi, fullPageSize, contentSize, contentSize, cultureInfo)
                : child.Measure(dpi, fullPageSize, contentSize, contentSize, cultureInfo);
            if (arrange)
                _arrangedChildSizes.Add(childSize);

            maxWidth = Math.Max(maxWidth, childSize.Width);
            totalHeight += childSize.Height;
        }

        return new PapercraftSize(maxWidth, totalHeight);
    }

    private void RenderBox(IDeferredCanvas canvas, float boxSize, float strokeThickness)
    {
        if (boxSize <= 0F)
            return;

        if (Fill.Alpha > 0)
            canvas.DrawRect(new Rectangle(0F, 0F, boxSize, boxSize), Fill);

        if (strokeThickness <= 0F)
            return;

        canvas.DrawLine(StrokeColor, strokeThickness, 0F, 0F, boxSize, 0F);
        canvas.DrawLine(StrokeColor, strokeThickness, boxSize, 0F, boxSize, boxSize);
        canvas.DrawLine(StrokeColor, strokeThickness, boxSize, boxSize, 0F, boxSize);
        canvas.DrawLine(StrokeColor, strokeThickness, 0F, boxSize, 0F, 0F);

        if (!Checked)
            return;

        canvas.DrawLine(
            CheckColor,
            strokeThickness,
            boxSize / 4F,
            boxSize / 2F,
            boxSize / 2F,
            boxSize * 3F / 4F);
        canvas.DrawLine(
            CheckColor,
            strokeThickness,
            boxSize / 2F,
            boxSize * 3F / 4F,
            boxSize * 3F / 4F,
            boxSize / 4F);
    }

    private void RenderLabel(
        IDeferredCanvas canvas,
        float dpi,
        string label,
        PapercraftSize parentSize,
        float boxSize)
    {
        var gap = GetGap(dpi, parentSize);
        using var state = canvas.CreateState();
        canvas.Translate(boxSize + gap, 0F);
        _textService.Draw(
            canvas,
            _labelTextStyle,
            dpi,
            label.AsSpan(),
            GetContentWidth(ArrangementInner.Width > 0F ? ArrangementInner.Width : parentSize.Width, boxSize, gap));
    }

    private PapercraftSize RenderChildren(
        IDeferredCanvas canvas,
        float dpi,
        PapercraftSize parentSize,
        CultureInfo cultureInfo,
        float boxSize)
    {
        var gap = GetGap(dpi, parentSize);
        var additionalWidth = 0F;
        var additionalHeight = 0F;
        using var state = canvas.CreateState();
        canvas.Translate(boxSize + gap, 0F);

        var contentParentSize = new PapercraftSize(GetContentWidth(parentSize.Width, boxSize, gap), parentSize.Height);
        foreach (var (child, arrangedChildSize) in Children.Zip(_arrangedChildSizes))
        {
            var childAdditionalSize = child.Render(canvas, dpi, contentParentSize, cultureInfo);
            if (childAdditionalSize.Width > 0F)
                additionalWidth = Math.Max(additionalWidth, boxSize + gap + childAdditionalSize.Width);
            additionalHeight += childAdditionalSize.Height;
            canvas.Translate(0F, arrangedChildSize.Height + childAdditionalSize.Height);
        }

        return new PapercraftSize(additionalWidth, additionalHeight);
    }

    private string GetLabel()
        => Label.Trim();

    private float GetBoxSize(float dpi, PapercraftSize bounds)
        => Math.Max(0F, Size.ToPixels(Math.Min(bounds.Width, bounds.Height), dpi));

    private float GetGap(float dpi, PapercraftSize bounds)
        => Math.Max(0F, Gap.ToPixels(bounds.Width, dpi));

    private float GetStrokeThickness(float dpi, float boxSize)
        => Math.Max(0F, StrokeThickness.ToPixels(boxSize, dpi));

    private static float GetContentWidth(float availableWidth, float boxSize, float gap)
        => Math.Max(0F, availableWidth - boxSize - gap);

    private static PapercraftSize WithContent(float boxSize, float gap, PapercraftSize contentSize)
        => new(boxSize + gap + contentSize.Width, Math.Max(boxSize, contentSize.Height));
}
