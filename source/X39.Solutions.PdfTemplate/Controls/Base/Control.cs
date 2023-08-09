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
            new Size(
                availableSize.Width - padding.Width - padding.Width - margin.Width - margin.Width,
                availableSize.Height - padding.Height - padding.Height - margin.Height - margin.Height),
            cultureInfo);
        MeasurementOuter = new Rectangle(
            0,
            0,
            measureResult.Width + padding.Width + padding.Width + margin.Width + margin.Width,
            measureResult.Height + padding.Height + padding.Height + margin.Height + margin.Height);
        Measurement = new Rectangle(
            margin.Left,
            margin.Top,
            measureResult.Width + padding.Width + padding.Width,
            measureResult.Height + padding.Height + padding.Height);
        MeasurementInner = new Rectangle(
            margin.Left + padding.Left,
            margin.Top + padding.Top,
            measureResult.Width,
            measureResult.Height);
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
            new Size(
                finalSize.Width - padding.Width - padding.Width - margin.Width - margin.Width,
                finalSize.Height - padding.Height - padding.Height - margin.Height - margin.Height),
            cultureInfo);
        ArrangementOuter = new Rectangle(
            0,
            0,
            measureResult.Width + padding.Width + padding.Width + margin.Width + margin.Width,
            measureResult.Height + padding.Height + padding.Height + margin.Height + margin.Height);
        Arrangement = new Rectangle(
            margin.Left,
            margin.Top,
            measureResult.Width + padding.Width + padding.Width,
            measureResult.Height + padding.Height + padding.Height);
        ArrangementInner = new Rectangle(
            margin.Left + padding.Left,
            margin.Top + padding.Top,
            measureResult.Width,
            measureResult.Height);
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
        var padding = Padding.ToRectangle(parentSize);
        var margin = Margin.ToRectangle(parentSize);
        canvas.PushState();
        try
        {
            if (Clip)
                canvas.ClipRect(Arrangement);
            canvas.Translate(ArrangementInner);
            var arrangedSize = new Size(
                parentSize.Width - padding.Width - padding.Width - margin.Width - margin.Width,
                parentSize.Height - padding.Height - padding.Height - margin.Height - margin.Height);
            PreRender(canvas, arrangedSize, cultureInfo);
            DoRender(canvas, arrangedSize, cultureInfo);
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