using System.ComponentModel;
using System.Globalization;
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

    /// <inheritdoc />
    public virtual Size Measure(
        in Size availableSize,
        CultureInfo cultureInfo)
    {
        var padding = Padding.ToRectangle(availableSize);
        var margin = Margin.ToRectangle(availableSize);
        var measureResult = DoMeasure(
            availableSize - padding - margin,
            cultureInfo);
        MeasurementOuter = new Rectangle(
            0,
            0,
            measureResult.Width + margin.Width,
            measureResult.Height + margin.Height);
        Measurement = new Rectangle(
            MeasurementOuter.Left + margin.Left,
            MeasurementOuter.Top + margin.Top,
            MeasurementOuter.Width - margin.Width,
            MeasurementOuter.Height - margin.Height);
        MeasurementInner = new Rectangle(
            Measurement.Left + padding.Left,
            Measurement.Top + padding.Top,
            Measurement.Width - padding.Width,
            Measurement.Height - padding.Height);
        return MeasurementOuter;
    }

    /// <inheritdoc cref="Measure"/>
    protected abstract Size DoMeasure(
        in Size availableSize,
        CultureInfo cultureInfo);

    /// <inheritdoc />
    public virtual Size Arrange(
        in Size finalSize,
        CultureInfo cultureInfo)
    {
        var padding = Padding.ToRectangle(finalSize);
        var margin = Margin.ToRectangle(finalSize);
        var measureResult = DoArrange(
            finalSize - padding - margin,
            cultureInfo);
        ArrangementOuter = new Rectangle(
            0,
            0,
            measureResult.Width + margin.Width,
            measureResult.Height + margin.Height);
        Arrangement = new Rectangle(
            ArrangementOuter.Left + margin.Left,
            ArrangementOuter.Top + margin.Top,
            ArrangementOuter.Width - margin.Width,
            ArrangementOuter.Height - margin.Height);
        ArrangementInner = new Rectangle(
            Arrangement.Left + padding.Left,
            Arrangement.Top + padding.Top,
            Arrangement.Width - padding.Width,
            Arrangement.Height - padding.Height);
        return ArrangementOuter;
    }

    /// <inheritdoc cref="Arrange"/>
    protected abstract Size DoArrange(
        in Size finalSize,
        CultureInfo cultureInfo);

    /// <inheritdoc />
    public virtual void Render(
        ICanvas canvas,
        in Size parentSize,
        CultureInfo cultureInfo)
    {
        canvas.PushState();
        try
        {
            if (Clip)
                canvas.ClipRect(Arrangement);
            canvas.Translate(ArrangementInner);
            PreRender(canvas, parentSize, cultureInfo);
            DoRender(canvas, parentSize, cultureInfo);
        }
        finally
        {
            canvas.PopState();
        }
    }

    /// <summary>
    /// Called before <see cref="DoRender"/> is called to allow for additional canvas state to be set.
    /// </summary>
    /// <param name="canvas">The canvas to render to.</param>
    /// <param name="parentSize">The size of the parent control.</param>
    /// <param name="cultureInfo">The culture info to use for rendering.</param>
    protected virtual void PreRender(ICanvas canvas, in Size parentSize, CultureInfo cultureInfo)
    {
        /* empty */
    }

    /// <inheritdoc cref="Render"/>
    protected abstract void DoRender(
        ICanvas canvas,
        in Size parentSize,
        CultureInfo cultureInfo);
}