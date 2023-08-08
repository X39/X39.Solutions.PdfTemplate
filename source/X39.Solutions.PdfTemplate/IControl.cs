using System.Globalization;
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
    /// <param name="availableSize">Available size that parent can give to the child. May be infinity (when parent wants to
    /// measure to content). This is soft constraint. Child can return bigger size to indicate that it wants bigger space and hope
    /// that parent can throw in pagination...</param>
    /// <param name="cultureInfo">The culture info to use for the measurement.</param>
    /// <returns>The desired size of the control.</returns>
    Size Measure(
        in Size availableSize,
        CultureInfo cultureInfo);

    /// <summary>
    /// Arrange the control within the given bounds.
    /// </summary>
    /// <param name="finalSize">The final size that element should use to arrange itself and its children.</param>
    /// <param name="cultureInfo">The culture info to use for the arrangement.</param>
    /// <returns>The actual size of the control.</returns>
    Size Arrange(
        in Size finalSize,
        CultureInfo cultureInfo);

    /// <summary>
    /// Draw the control.
    /// </summary>
    /// <param name="canvas">The canvas to draw on.</param>
    /// <param name="parentSize">The size of the parent control.</param>
    /// <param name="cultureInfo">The culture info to use for the drawing.</param>
    void Render(
        ICanvas canvas,
        in Size parentSize,
        CultureInfo cultureInfo);
}