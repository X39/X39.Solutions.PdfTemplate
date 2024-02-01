using System.ComponentModel;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Attributes;
using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate.Controls.Base;

/// <summary>
/// Base class for all controls
/// </summary>
public abstract class Control : IControl
{
    /// <summary>
    /// The margin of the control.
    /// </summary>
    [Parameter]
    public Thickness Margin { get; set; }

    /// <summary>
    /// The padding of the control.
    /// </summary>
    [Parameter]
    public Thickness Padding { get; set; }

    /// <summary>
    /// Whether the control should clip its content.
    /// </summary>
    /// <remarks>
    /// Disabling clipping may cause the control to overflow its bounds.
    /// </remarks>
    [Parameter]
    public bool Clip { get; set; } = true;

    /// <summary>
    /// The arrangement of the control, having padding and margin removed already.
    /// </summary>
    /// <example>
    /// <code>
    /// this = arrangement - margin - padding
    /// </code>
    /// </example>
    /// <remarks>
    /// This property is set by <see cref="Arrange"/>.
    /// </remarks>
    public Rectangle ArrangementInner
    {
        get;
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        private set;
    }

    /// <summary>
    /// The arrangement of the control, having margin removed already.
    /// </summary>
    /// <example>
    /// <code>
    /// this = arrangement - margin
    /// </code>
    /// </example>
    /// <remarks>
    /// This property is set by <see cref="Arrange"/>.
    /// </remarks>
    public Rectangle Arrangement
    {
        get;
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        private set;
    }

    /// <summary>
    /// The arrangement of the control without margin or padding removed.
    /// </summary>
    /// <example>
    /// <code>
    /// this = arrangement
    /// </code>
    /// </example>
    /// <remarks>
    /// This property is set by <see cref="Arrange"/>.
    /// </remarks>
    public Rectangle ArrangementOuter
    {
        get;
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        private set;
    }

    /// <summary>
    /// The measurement of the control, having padding and margin removed already.
    /// </summary>
    /// <example>
    /// <code>
    /// this = measurement - margin - padding
    /// </code>
    /// </example>
    /// <remarks>
    /// This property is set by <see cref="Measure"/>.
    /// </remarks>
    public Rectangle MeasurementInner
    {
        get;
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        private set;
    }

    /// <summary>
    /// The measurement of the control, having margin removed already.
    /// </summary>
    /// <example>
    /// <code>
    /// this = measurement - margin
    /// </code>
    /// </example>
    /// <remarks>
    /// This property is set by <see cref="Measure"/>.
    /// </remarks>
    public Rectangle Measurement
    {
        get;
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        private set;
    }

    /// <summary>
    /// The measurement of the control without margin or padding removed.
    /// </summary>
    /// <example>
    /// <code>
    /// this = measurement
    /// </code>
    /// </example>
    /// <remarks>
    /// This property is set by <see cref="Measure"/>.
    /// </remarks>
    public Rectangle MeasurementOuter
    {
        get;
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        private set;
    }

    /// <summary>
    /// The size of the control available for the frame as given by the parent control.
    /// </summary>
    public Size FramedSize
    {
        get;
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        private set;
    }

    /// <summary>
    /// The size of the control remaining for the frame as given by the parent control.
    /// </summary>
    public Size RemainingSize
    {
        get;
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        private set;
    }

    /// <inheritdoc />
    public virtual Size Measure(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        var padding = Padding.ToRectangle(fullPageSize, dpi);
        var margin = Margin.ToRectangle(fullPageSize, dpi);
        var measureResult = DoMeasure(
            dpi,
            ToSize(fullPageSize, margin),
            ToSize(framedPageSize, margin),
            ToSize(remainingSize, margin, padding),
            cultureInfo);
        MeasurementOuter = new Rectangle(
            0,
            0,
            measureResult.Width + padding.Right + margin.Right,
            measureResult.Height + padding.Bottom + margin.Bottom);
        Measurement = new Rectangle(
            margin.Left,
            margin.Top,
            measureResult.Width + padding.Right,
            measureResult.Height + padding.Bottom);
        MeasurementInner = new Rectangle(
            margin.Left + padding.Left,
            margin.Top + padding.Top,
            measureResult.Width,
            measureResult.Height);
        return MeasurementOuter;
    }

    private static Size ToSize(Size fullPageSize, Rectangle margin, Rectangle padding = default)
    {
        return new Size(
            fullPageSize.Width - padding.Width - padding.Width - margin.Width - margin.Width,
            fullPageSize.Height - padding.Height - padding.Height - margin.Height - margin.Height);
    }

    /// <inheritdoc cref="Measure"/>
    protected abstract Size DoMeasure(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo);

    /// <inheritdoc />
    public virtual Size Arrange(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        FramedSize    = framedPageSize;
        RemainingSize = framedPageSize;
        var padding = Padding.ToRectangle(remainingSize, dpi);
        var margin = Margin.ToRectangle(remainingSize, dpi);
        var measureResult = DoArrange(
            dpi,
            ToSize(fullPageSize, margin),
            ToSize(framedPageSize, margin),
            ToSize(remainingSize, margin, padding),
            cultureInfo);
        ArrangementOuter = new Rectangle(
            0,
            0,
            measureResult.Width + padding.Right + margin.Right,
            measureResult.Height + padding.Bottom + margin.Bottom);
        Arrangement = new Rectangle(
            margin.Left,
            margin.Top,
            measureResult.Width + padding.Right,
            measureResult.Height + padding.Bottom);
        ArrangementInner = new Rectangle(
            margin.Left + padding.Left,
            margin.Top + padding.Top,
            measureResult.Width,
            measureResult.Height);
        return ArrangementOuter;
    }

    /// <inheritdoc cref="Arrange"/>
    protected abstract Size DoArrange(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo);

    /// <inheritdoc />
    public virtual Size Render(
        ICanvas     canvas,
        float       dpi,
        in Size     parentSize,
        CultureInfo cultureInfo
    )
    {
        var padding = Padding.ToRectangle(parentSize, dpi);
        var margin  = Margin.ToRectangle(parentSize, dpi);
        using (canvas.CreateState())
        {
            var arrangedSize = new Size(
                parentSize.Width - padding.Width - padding.Width - margin.Width - margin.Width,
                parentSize.Height - padding.Height - padding.Height - margin.Height - margin.Height
            );
            var (width, height) = PreRender(canvas, dpi, arrangedSize, cultureInfo);
            var additionalHeight = height;
            var additionalWidth  = width;
            if (Clip)
                canvas.Clip(Arrangement + new Size(additionalWidth, additionalHeight));
            canvas.Translate(ArrangementInner);
            (width, height)  = DoRender(canvas, dpi, arrangedSize, cultureInfo);
            additionalHeight = height;
            additionalWidth  = width;
            return new Size(additionalWidth, additionalHeight);
        }
    }

    /// <summary>
    /// Called before <see cref="DoRender"/> is called to allow for additional canvas state to be set.
    /// </summary>
    /// <param name="canvas">The canvas to render to.</param>
    /// <param name="dpi"></param>
    /// <param name="parentSize">The size of the parent control.</param>
    /// <param name="cultureInfo">The culture info to use for rendering.</param>
    /// <returns>Additional size used by the control. Note that this is only used for Clip and is discarded otherwise.</returns>
    protected virtual Size PreRender(
        ICanvas     canvas,
        float       dpi,
        in Size     parentSize,
        CultureInfo cultureInfo)
    {
        return Size.Zero;
    }

    /// <inheritdoc cref="Render"/>
    protected abstract Size DoRender(ICanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo);
}