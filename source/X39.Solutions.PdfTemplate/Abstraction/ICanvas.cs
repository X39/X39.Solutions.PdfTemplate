using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate.Abstraction;

/// <summary>
/// A canvas for drawing on.
/// </summary>
public interface ICanvas
{
    /// <summary>
    /// This call saves the current matrix, clip, and draw filter,
    /// and pushes a copy onto a private stack.
    /// Subsequent calls to translate, scale, rotate, skew, concatenate or clipping path or drawing filter
    /// all operate on this copy.
    /// When the balancing call to <see cref="PopState"/> is made,
    /// the previous matrix, clipping, and drawing filters are restored.
    /// </summary>
    void PushState();
    /// <summary>
    /// Modify the current clip with the specified <see cref="Rectangle"/>.
    /// </summary>
    /// <param name="rectangle">The <see cref="Rectangle"/> to combine with the current clip.</param>
    void Clip(Rectangle rectangle);
    /// <summary>
    /// This call balances a previous call to <see cref="PushState"/>,
    /// and is used to remove all modifications to the matrix,
    /// clip and draw filter state since the last <see cref="PushState"/> call.
    /// It is an error to <see cref="PopState"/> more times than was previously <see cref="PushState"/>.
    /// </summary>
    void PopState();
    /// <summary>
    /// Draw a line segment with the specified <see cref="Color"/>, thickness, and start and end points.
    /// </summary>
    /// <param name="color">The <see cref="Color"/> to paint the line.</param>
    /// <param name="thickness">The thickness of the line.</param>
    /// <param name="startX">The x-coordinate of the start point.</param>
    /// <param name="startY">The y-coordinate of the start point.</param>
    /// <param name="endX">The x-coordinate of the end point.</param>
    /// <param name="endY">The y-coordinate of the end point.</param>
    void DrawLine(Color color, float thickness, float startX, float startY, float endX, float endY);

    /// <summary>
    /// Pre-concatenates the current matrix with the specified translation.
    /// </summary>
    /// <param name="point">The distance to translate.</param>
    void Translate(Point point);

    /// <summary>
    ///     Draws text on the canvas at the specified coordinates.
    /// </summary>
    /// <param name="text">The text to draw.</param>
    /// <param name="x">The x-coordinate of the origin of the text being drawn.</param>
    /// <param name="y">The y-coordinate of the origin of the text being drawn.</param>
    /// <param name="textStyle">The <see cref="TextStyle"/> to use when drawing the text.</param>
    void DrawText(TextStyle textStyle, string text, float x, float y);
}