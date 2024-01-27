using SkiaSharp;
using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate.Abstraction;

/// <summary>
/// A canvas for drawing on.
/// </summary>
public interface ICanvas
{
    /// <summary>
    /// Represents the current translation of the canvas.
    /// </summary>
    /// <remarks>
    /// The translation is a <see cref="Point"/> value that represents the current position of the canvas.
    /// It specifies the distance to move all subsequent drawing operations on the canvas.
    /// </remarks>
    public Point Translation { get; }
    
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
    /// <param name="textStyle">The <see cref="TextStyle"/> to use when drawing the text.</param>
    /// <param name="dpi"></param>
    /// <param name="text">The text to draw.</param>
    /// <param name="x">The x-coordinate of the origin of the text being drawn.</param>
    /// <param name="y">The y-coordinate of the origin of the text being drawn.</param>
    void DrawText(TextStyle textStyle, float dpi, string text, float x, float y);

    /// <summary>
    ///     Draws a rectangle on the canvas at the coordinates specified
    ///     by <see cref="Rectangle.Left"/> and <see cref="Rectangle.Top"/>.
    /// </summary>
    /// <param name="rectangle">The rectangle position.</param>
    /// <param name="color">The fill color of the rectangle.</param>
    void DrawRect(Rectangle rectangle, Color color);

    /// <summary>
    ///     Draws a bitmap on the canvas.
    /// </summary>
    /// <param name="bitmap">The bitmap to draw.</param>
    /// <param name="rectangle">The region to draw the bitmap into.</param>
    void DrawBitmap(byte[] bitmap, Rectangle rectangle);

    /// <summary>
    ///     Draws a bitmap on the canvas.
    /// </summary>
    /// <param name="bitmap">The SkiaSharp bitmap to draw.</param>
    /// <param name="arrangementInner">The region to draw the bitmap into.</param>
    void DrawBitmap(SKBitmap bitmap, Rectangle arrangementInner);
}