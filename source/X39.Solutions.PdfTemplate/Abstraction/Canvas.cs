namespace X39.Solutions.PdfTemplate.Abstraction;

/// <summary>
/// Canvas extension methods.
/// </summary>
[PublicAPI]
public static class Canvas
{
    /// <summary>
    /// This call saves the current matrix, clip, and draw filter,
    /// and pushes a copy onto a private stack.
    /// Subsequent calls to translate, scale, rotate, skew, concatenate or clipping path or drawing filter
    /// all operate on this copy.
    /// When the balancing call is done by disposing the return value,
    /// the previous matrix, clipping, and drawing filters are restored.
    /// </summary>
    public static IDisposable CreateState(this IDrawableCanvas canvas)
    {
        canvas.PushState();
        return new Disposable(canvas.PopState);
    }

    /// <summary>
    /// Calculates the remaining height of the page on the canvas.
    /// </summary>
    /// <param name="canvas">The canvas on which the page is drawn.</param>
    /// <param name="heightPerPage">The height of each page.</param>
    /// <returns>The remaining height on the page.</returns>
    public static float GetRemainingPageHeight(this IDeferredCanvas canvas, float heightPerPage)
    {
        var usedHeight = canvas.GetUsedPageHeight(heightPerPage);
        return heightPerPage - usedHeight;
    }

    /// <summary>
    /// Calculates the used height of the page on the canvas.
    /// </summary>
    /// <param name="canvas">The canvas on which the page is drawn.</param>
    /// <param name="heightPerPage">The height of each page.</param>
    /// <returns>The amount of height used on the page.</returns>
    public static float GetUsedPageHeight(this IDeferredCanvas canvas, float heightPerPage)
    {
        var y = canvas.Translation.Y;
        var multiplier = (int) (y / heightPerPage);
        return y - multiplier * heightPerPage;
    }
}