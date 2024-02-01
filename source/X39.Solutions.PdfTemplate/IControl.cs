using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate;

/// <summary>
/// A control that can be drawn on a page.
/// </summary>
[PublicAPI]
public interface IControl
{
    /// <summary>
    /// Calculate the desired size of the control.
    /// </summary>
    /// <remarks>
    /// Desired size may overflow the bounds.
    /// </remarks>
    /// <param name="dpi"></param>
    /// <param name="fullPageSize">
    ///     The full size of a single page, not including header and footer (in short: the printable area).
    /// </param>
    /// <param name="framedPageSize">
    ///     The full size of a single page framed to the controls perspective.
    ///     A parent control may limit the size of this value to the size of the page it is on.
    /// </param>
    /// <param name="remainingSize">
    ///     Size on the current page, left for the control to use.
    /// </param>
    /// <param name="cultureInfo">The culture info to use for the measurement.</param>
    /// <returns>The desired size of the control.</returns>
    Size Measure(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo);

    /// <summary>
    /// Arrange the control within the given bounds.
    /// </summary>
    /// <param name="dpi"></param>
    /// <param name="fullPageSize">
    ///     The full size of a single page, not including header and footer (in short: the printable area).
    /// </param>
    /// <param name="framedPageSize">
    ///     The full size of a single page framed to the controls perspective.
    ///     A parent control may limit the size of this value to the size of the page it is on.
    /// </param>
    /// <param name="remainingSize">
    ///     Size on the current page, left for the control to use.
    ///     A control may not use more space than this value.
    /// </param>
    /// <param name="cultureInfo">The culture info to use for the arrangement.</param>
    /// <returns>The actual size of the control.</returns>
    Size Arrange(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo);

    /// <summary>
    /// Draw the control.
    /// </summary>
    /// <param name="canvas">The canvas to draw on.</param>
    /// <param name="dpi"></param>
    /// <param name="parentSize">The size of the parent control.</param>
    /// <param name="cultureInfo">The culture info to use for the drawing.</param>
    /// <returns>Additional size used by the control.</returns>
    Size Render(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo);
}